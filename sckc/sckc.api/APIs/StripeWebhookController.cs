using sckc.api.Extensions;
using sckc.api.Models;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace sckc.api.APIs
{
    // Stripe calls this when a Checkout payment (see BookingsController.payNow) goes through, and it emails the
    // payments mailbox. Stripe's dashboard must send checkout.session.completed and
    // checkout.session.async_payment_succeeded to https://<site>/api/stripe/webhook.
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        public const string WebhookSecretFile = "data/stripe_webhook_secret.data";
        public const string StripeApiKeyFile = "data/stripe_apikey.data";

        // What was bought, by session id; replaced by the tests so they don't call Stripe
        public static Func<string, Task<List<string>>> LineItemReader { get; set; } = ReadLineItemsFromStripe;

        [Route("api/stripe/webhook")]
        [HttpPost]
        public async Task<IActionResult> Receive()
        {
            var secretPath = Helper.MapPath(WebhookSecretFile);
            if (!System.IO.File.Exists(secretPath))
            {
                throw new InvalidOperationException($"Stripe webhook can't be checked: {WebhookSecretFile} is missing");
            }

            var signature = Request.Headers["Stripe-Signature"].ToString();
            if (string.IsNullOrWhiteSpace(signature))
            {
                return BadRequest("Missing Stripe-Signature header");
            }

            var json = await new StreamReader(Request.Body, Encoding.UTF8).ReadToEndAsync();

            Event stripeEvent;
            try
            {
                // The signature proves the request came from Stripe. The endpoint in Stripe's dashboard is set to the
                // API version this Stripe.net release is built for (2026-09-30.endive); a mismatch is logged below
                // rather than refused, as the few session fields read here are stable across versions
                stripeEvent = EventUtility.ConstructEvent(json, signature,
                    System.IO.File.ReadAllText(secretPath).Trim(), throwOnApiVersionMismatch: false);
            }
            catch (StripeException ex)
            {
                return BadRequest(ex.Message);
            }

            if (stripeEvent.ApiVersion != StripeConfiguration.ApiVersion)
            {
                HttpContext?.RequestServices.GetService<ILogger<StripeWebhookController>>()?.LogWarning(
                    "Stripe webhook sent API version {EventVersion} but Stripe.net expects {LibraryVersion}; set the endpoint to match",
                    stripeEvent.ApiVersion, StripeConfiguration.ApiVersion);
            }

            // Card payments are paid on completion; delayed methods (e.g. bank debits) complete unpaid and send
            // async_payment_succeeded once the money arrives, so each payment is emailed exactly once
            var paidNow = stripeEvent.Type == EventTypes.CheckoutSessionCompleted && (stripeEvent.Data.Object as Session)?.PaymentStatus == "paid";
            var paidLater = stripeEvent.Type == EventTypes.CheckoutSessionAsyncPaymentSucceeded;
            if ((paidNow || paidLater) && stripeEvent.Data.Object is Session session)
            {
                // A failure here returns an error, so Stripe retries the event later
                await Helper.SendMail(await BuildPaymentEmail(session, stripeEvent.Livemode));
            }

            // Any other event is acknowledged so Stripe doesn't keep resending it
            return Ok();
        }

        private async Task<EmailMessage> BuildPaymentEmail(Session session, bool live)
        {
            var metadata = session.Metadata ?? new Dictionary<string, string>();
            string Meta(string key) => metadata.TryGetValue(key, out var value) ? value : null;

            List<string> items;
            try
            {
                items = await LineItemReader(session.Id);
            }
            catch (Exception ex)
            {
                // Still worth sending: the amount and the booker's details are in the event itself
                HttpContext?.RequestServices.GetService<ILogger<StripeWebhookController>>()?.LogWarning(ex, "Couldn't read line items for {Session}", session.Id);
                items = new List<string>();
            }

            var amount = FormatAmount(session.AmountTotal, session.Currency);
            var name = Meta("Name") ?? session.CustomerDetails?.Name;
            var email = Meta("Email") ?? session.CustomerDetails?.Email ?? session.CustomerEmail;
            var subject = $"Payment received: {amount} from {name ?? email ?? "a customer"}";
            if (!live)
            {
                subject = $"***TEST TEST {subject} TEST TEST***";
            }

            var rows = new List<(string, string)>
            {
                ("Amount", amount),
                ("Event", Meta("Event")),
                ("Date", Meta("Date")),
                ("Name", name),
                ("Email", email),
                ("Phone", Meta("TelNo")),
                ("Payment", session.PaymentIntentId),
            }.Where(r => !string.IsNullOrWhiteSpace(r.Item2)).ToList();

            var html = new StringBuilder("<p>A payment has been received through the website.</p><table cellpadding=\"4\">");
            foreach (var (label, value) in rows)
            {
                html.Append($"<tr><th align=\"left\">{E(label)}</th><td>{E(value)}</td></tr>");
            }
            html.Append("</table>");
            if (items.Count > 0)
            {
                html.Append("<p><strong>Paid for</strong></p><ul>");
                html.Append(string.Concat(items.Select(i => $"<li>{E(i)}</li>")));
                html.Append("</ul>");
            }
            if (!string.IsNullOrEmpty(session.PaymentIntentId))
            {
                var dashboard = $"https://dashboard.stripe.com/{(live ? "" : "test/")}payments/{Uri.EscapeDataString(session.PaymentIntentId)}";
                html.Append($"<p><a href=\"{E(dashboard)}\">View the payment in Stripe</a></p>");
            }

            var payments = new EmailAddress(Constants.PaymentsEmailAddress, Constants.PaymentsEmailName);
            var msg = new EmailMessage(new EmailAddress(Constants.PaymentsFromAddress, Constants.PaymentsEmailName), payments, subject, html.ToString());
            if (!string.IsNullOrWhiteSpace(email))
            {
                msg.ReplyTo = new EmailAddress(email, name);
            }
            return msg;
        }

        private static async Task<List<string>> ReadLineItemsFromStripe(string sessionId)
        {
            var keyPath = Helper.MapPath(StripeApiKeyFile);
            if (!System.IO.File.Exists(keyPath))
            {
                return new List<string>();
            }

            var client = new StripeClient(System.IO.File.ReadAllText(keyPath).Trim());
            var session = await new SessionService(client).GetAsync(sessionId, new SessionGetOptions { Expand = new List<string> { "line_items" } });
            return session.LineItems?.Data.Select(li => $"{li.Quantity} × {li.Description} ({FormatAmount(li.AmountTotal, li.Currency)})").ToList()
                ?? new List<string>();
        }

        private static string FormatAmount(long? minorUnits, string currency)
        {
            var value = (minorUnits ?? 0) / 100m;
            return string.Equals(currency, "gbp", StringComparison.OrdinalIgnoreCase)
                ? value.ToString("£#,##0.00", CultureInfo.InvariantCulture)
                : $"{value.ToString("#,##0.00", CultureInfo.InvariantCulture)} {currency?.ToUpperInvariant()}";
        }

        private static string E(string value) => WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
