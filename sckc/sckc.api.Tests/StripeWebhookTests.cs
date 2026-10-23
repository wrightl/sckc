using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sckc.api.APIs;
using sckc.api.Extensions;
using sckc.api.Models;

namespace sckc.api.Tests
{
    // Stripe's payment webhook, sent signed events through the real pipeline; no email or Stripe call leaves the test
    [TestClass]
    public class StripeWebhookTests
    {
        private const string Secret = "whsec_test_secret";

        private Func<EmailMessage, Task<bool>> originalSender;
        private Func<string, Task<List<string>>> originalReader;
        private string root;
        private List<EmailMessage> sent;
        private WebApplicationFactory<Program> factory;

        [TestInitialize]
        public void Setup()
        {
            originalSender = Helper.MailSender;
            originalReader = StripeWebhookController.LineItemReader;
            root = Path.Combine(Path.GetTempPath(), "sckc-stripe-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "data"));
            File.WriteAllText(Path.Combine(root, "data", "stripe_webhook_secret.data"), Secret + "\n");
            File.WriteAllText(Path.Combine(root, "index.html"), "<html>site</html>");
            factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseContentRoot(root));
            sent = new List<EmailMessage>();
            Helper.MailSender = message => { sent.Add(message); return Task.FromResult(true); };
            StripeWebhookController.LineItemReader = _ => Task.FromResult(new List<string> { "2 × Adult pool session (£12.00)" });
        }

        [TestCleanup]
        public void Cleanup()
        {
            factory.Dispose();
            Helper.MailSender = originalSender;
            StripeWebhookController.LineItemReader = originalReader;
            Directory.Delete(root, recursive: true);
        }

        private static string EventJson(string type, string paymentStatus = "paid", bool live = true) => $@"{{
  ""id"": ""evt_test_1"",
  ""object"": ""event"",
  ""api_version"": ""2026-09-30.endive"",
  ""created"": 1792700000,
  ""livemode"": {(live ? "true" : "false")},
  ""pending_webhooks"": 1,
  ""request"": {{ ""id"": null, ""idempotency_key"": null }},
  ""type"": ""{type}"",
  ""data"": {{ ""object"": {{
    ""id"": ""cs_test_123"",
    ""object"": ""checkout.session"",
    ""amount_total"": 1200,
    ""currency"": ""gbp"",
    ""mode"": ""payment"",
    ""payment_status"": ""{paymentStatus}"",
    ""payment_intent"": ""pi_test_456"",
    ""customer_details"": {{ ""email"": ""angel@example.com"", ""name"": ""Angel"" }},
    ""metadata"": {{ ""Event"": ""PoolSession"", ""Date"": ""Sun 1 Nov 2026"", ""Name"": ""Angel <b>Smith</b>"", ""TelNo"": ""0114 000000"", ""Email"": ""angel@example.com"", ""People"": ""1"" }}
  }} }}
}}";

        // Stripe's scheme: v1 = HMAC-SHA256 of "{timestamp}.{body}" with the endpoint's signing secret
        private static string Signature(string json, string secret = Secret, DateTimeOffset? at = null)
        {
            var timestamp = (at ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds();
            var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes($"{timestamp}.{json}"));
            return $"t={timestamp},v1={Convert.ToHexString(hash).ToLowerInvariant()}";
        }

        private async Task<HttpResponseMessage> PostAsync(string json, string signature)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/stripe/webhook")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };
            if (signature != null)
            {
                request.Headers.TryAddWithoutValidation("Stripe-Signature", signature);
            }
            return await factory.CreateClient().SendAsync(request);
        }

        // The webhook endpoint in Stripe's dashboard is fixed at this version. If an upgrade of Stripe.net changes it,
        // create a new endpoint at the new version (Stripe can't change an existing one), then update this test.
        [TestMethod]
        public void StripeLibrary_MatchesTheWebhookEndpointVersion()
        {
            Assert.AreEqual("2026-09-30.endive", Stripe.StripeConfiguration.ApiVersion);
        }

        [TestMethod]
        public async Task CompletedPayment_EmailsThePaymentsMailbox()
        {
            var json = EventJson("checkout.session.completed");

            var response = await PostAsync(json, Signature(json));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var email = sent.Single();
            Assert.AreEqual("payment@sheffieldcitykayakclub.co.uk", email.To.Single().Email);
            Assert.AreEqual(Constants.PaymentsFromAddress, email.From.Email);
            Assert.AreEqual("Payment received: £12.00 from Angel <b>Smith</b>", email.Subject);
            Assert.AreEqual("angel@example.com", email.ReplyTo.Email);
            StringAssert.Contains(WebUtility.HtmlDecode(email.Html), "PoolSession");
            StringAssert.Contains(WebUtility.HtmlDecode(email.Html), "Sun 1 Nov 2026");
            StringAssert.Contains(WebUtility.HtmlDecode(email.Html), "0114 000000");
            StringAssert.Contains(WebUtility.HtmlDecode(email.Html), "2 × Adult pool session (£12.00)");
            StringAssert.Contains(WebUtility.HtmlDecode(email.Html), "https://dashboard.stripe.com/payments/pi_test_456");
            StringAssert.Contains(email.Html, "Angel &lt;b&gt;Smith&lt;/b&gt;");
        }

        [TestMethod]
        public async Task TestModePayment_IsMarkedAsATest()
        {
            var json = EventJson("checkout.session.completed", live: false);

            await PostAsync(json, Signature(json));

            StringAssert.StartsWith(sent.Single().Subject, "***TEST TEST Payment received");
            StringAssert.Contains(sent.Single().Html, "https://dashboard.stripe.com/test/payments/pi_test_456");
        }

        [TestMethod]
        public async Task DelayedPayment_IsEmailedWhenTheMoneyArrivesNotBefore()
        {
            var completed = EventJson("checkout.session.completed", paymentStatus: "unpaid");
            Assert.AreEqual(HttpStatusCode.OK, (await PostAsync(completed, Signature(completed))).StatusCode);
            Assert.AreEqual(0, sent.Count);

            var succeeded = EventJson("checkout.session.async_payment_succeeded");
            Assert.AreEqual(HttpStatusCode.OK, (await PostAsync(succeeded, Signature(succeeded))).StatusCode);
            Assert.AreEqual(1, sent.Count);
        }

        [TestMethod]
        public async Task OtherEvents_AreAcknowledgedWithoutAnEmail()
        {
            var json = EventJson("checkout.session.expired", paymentStatus: "unpaid");

            Assert.AreEqual(HttpStatusCode.OK, (await PostAsync(json, Signature(json))).StatusCode);
            Assert.AreEqual(0, sent.Count);
        }

        [TestMethod]
        public async Task UnsignedOrForgedRequests_AreRejected()
        {
            var json = EventJson("checkout.session.completed");

            Assert.AreEqual(HttpStatusCode.BadRequest, (await PostAsync(json, null)).StatusCode);
            Assert.AreEqual(HttpStatusCode.BadRequest, (await PostAsync(json, Signature(json, "whsec_someone_else"))).StatusCode);
            Assert.AreEqual(HttpStatusCode.BadRequest, (await PostAsync(json.Replace("1200", "1"), Signature(json))).StatusCode);
            Assert.AreEqual(HttpStatusCode.BadRequest, (await PostAsync(json, Signature(json, at: DateTimeOffset.UtcNow.AddHours(-1)))).StatusCode);
            Assert.AreEqual(0, sent.Count);
        }

        [TestMethod]
        public async Task FailedEmail_ReturnsAnErrorSoStripeRetries()
        {
            Helper.MailSender = _ => throw new Exception("Resend is down");
            var json = EventJson("checkout.session.completed");

            var response = await PostAsync(json, Signature(json));

            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [TestMethod]
        public async Task MissingLineItems_StillSendsTheEmail()
        {
            StripeWebhookController.LineItemReader = _ => throw new Exception("Stripe is down");
            var json = EventJson("checkout.session.completed");

            Assert.AreEqual(HttpStatusCode.OK, (await PostAsync(json, Signature(json))).StatusCode);
            StringAssert.Contains(WebUtility.HtmlDecode(sent.Single().Html), "£12.00");
        }
    }
}
