using sckc.api.Extensions;
using sckc.core.Models;
using SendGrid.Helpers.Mail;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace sckc.api.APIs
{
    public class BookingsController : ApiController
    {
        public BookingsController()
        {
            StripeConfiguration.ApiKey = System.IO.File.ReadAllText(Helper.MapPath("data/stripe.data")).Trim();
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
                var from = new EmailAddress("bookings@sheffieldcitykayakclub.co.uk", "Booking Request");
                var to = new EmailAddress("bookings@sheffieldcitykayakclub.co.uk", "Booking Request");
                var subject = $"Booking request for {info.Event} on {info.Date}";
                var htmlContent = $"From: {info.Name}<br/>Email: <a href=\"mailto:{info.Email}\">{info.Email}</a><br/>TelNo: {info.TelNo}<br/>Message:<br/>{info.Message}<br/><br/>" +
                    $"People:<br/>{ConvertToMessage(info.Items)}";

                if (!info.isLiveBooking)
                {
                    subject = $"***TEST TEST {subject} TEST TEST***";
                }

                var msg = MailHelper.CreateSingleEmail(from, to, subject, htmlContent, htmlContent);
                msg.ReplyTo = new EmailAddress(info.Email);

                var response = await Helper.SendMail(msg);

                // Log the booking request
                HttpClient client = new HttpClient();
                var result = await client.PostAsJsonAsync("https://sckc.azurewebsites.net/api/booking", info);


                if (response)
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