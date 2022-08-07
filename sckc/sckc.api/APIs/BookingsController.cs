using sckc.api.Extensions;
using sckc.core.Models;
using SendGrid.Helpers.Mail;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace sckc.api.APIs
{
    public class BookingsController : ApiController
    {
        public BookingsController()
        {
            StripeConfiguration.ApiKey = System.IO.File.ReadAllText(Helper.MapPath("data/stripe_apikey.data")).Trim();
        }

        [Route("api/Book")]
        [HttpPost]
        public async Task<IHttpActionResult> Book(BookingDto info)
        {
            if (info.payNow)
            {
                return Ok(await this.payNow(info));
            }
            else
            {
                var from = new EmailAddress(Constants.BookingsEmailAddress, Constants.BookingsEmailName);
                var to = new EmailAddress(Constants.BookingsEmailAddress, Constants.BookingsEmailName);
                var subject = $"Booking request for {info.Event} on {info.Date}";
                var htmlContent = $"From: {info.Name}<br/>Email: <a href=\"mailto:{info.Email}\">{info.Email}</a><br/>TelNo: {info.TelNo}<br/>Message:<br/>{info.Message}<br/><br/>" +
                    $"People:<br/>{ConvertToMessage(info.Items)}";

                if (!info.isLive)
                {
                    subject = $"***TEST TEST {subject} TEST TEST***";
                }

                var msg = MailHelper.CreateSingleEmail(from, to, subject, htmlContent, htmlContent);
                msg.ReplyTo = new EmailAddress(info.Email.Trim());

                var response = await Helper.SendMail(msg);

                try
                {
                    // Log the booking request
                    HttpClient client = new HttpClient();
                    var result = await client.PostAsJsonAsync($"{Constants.AdminSiteBaseUrl}payments/booking", info);

                    if (!response)
                        throw new Exception(await result.Content.ReadAsStringAsync());
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
            }

            return Ok();
        }

        [Route("api/BookingEnquiry")]
        [HttpPost]
        public async Task<IHttpActionResult> SendBookingEnquiry(BookingRequestDto info)
        {
            var from = new EmailAddress(Constants.BookingRequestEmailAddress, Constants.BookingRequestEmailName);
            var to = new EmailAddress(Constants.BookingRequestEmailAddress, Constants.BookingRequestEmailName);
            var subject = $"Booking request for {info.Event} on {info.Date}";
            var htmlContent = $"From: {info.Name}<br/>Email: <a href=\"mailto:{info.Email}\">{info.Email}</a><br/>Number of people: {info.Number}<br/>People: {info.Names}<br/>TelNo: {info.TelNo}<br/>Message:<br/>{info.Message}";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, htmlContent, htmlContent);
            msg.ReplyTo = new EmailAddress(info.Email);

            var response = await Helper.SendMail(msg);

            if (response)
            {
                // Log the booking request
                HttpClient client = new HttpClient();
                var result = await client.PostAsJsonAsync($"{Constants.AdminSiteBaseUrl}payments/enquiry", info);

                // Send template email
                to = new EmailAddress(info.Email.Trim());
                subject = $"Booking request for {info.Event} on {info.Date}";

                if (!info.isLive)
                {
                    subject = $"***TEST TEST {subject} TEST TEST***";
                }

                var autoReplyContent = Helper.ReplaceTemplatePlaceholders(System.IO.File.ReadAllText(Helper.MapPath($"Resources/{info.Type}_booking_request_response.html")).Trim(), info);

                msg = MailHelper.CreateSingleEmail(from, to, subject, autoReplyContent, autoReplyContent);

                await Helper.SendMail(msg);

                return Ok("Thanks for getting in touch. Someone will reply to your email soon");
            }
            return InternalServerError();
        }

        private async Task<string> payNow(BookingDto info)
        {
            var items = ConvertItems(info.Items);

            var uri = (new Uri(this.ControllerContext.Request.Headers.Referrer, "./")).ToString();

            var options = new SessionCreateOptions
            {
                LineItems = items,
                Mode = "payment",
                CustomerEmail = info.Email,
                SuccessUrl = $"{uri}bookingsuccess",
                CancelUrl = $"{uri}events",
                Metadata = new Dictionary<string, string>()
                {
                    { "Event", info.Event.Replace(" ", string.Empty) },
                    { "Date", info.Date },
                    { "Name", info.Name },
                    { "TelNo", info.TelNo },
                    { "Email", info.Email },
                    { "People", items.Count.ToString() }
                }
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return session.Url;
        }

        private List<SessionLineItemOptions> ConvertItems(List<BookingItemDto> items)
        {
            return items.Where(x => x.Quantity > 0).Select(x => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal = x.Cost * 100,
                    Currency = "gbp",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = x.Description,
                    },

                },
                Quantity = x.Quantity,
            }).ToList();
        }

        private string ConvertToMessage(List<BookingItemDto> items)
        {
            return string.Join("<br/>", items.Where(x => x.Quantity > 0).Select(x => $"{x.Quantity} {x.Description}").ToList());
        }
    }
}