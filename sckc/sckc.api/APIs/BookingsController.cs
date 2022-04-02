using sckc.api.Extensions;
using sckc.api.Models;
using Stripe;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var items = ConvertItems(info.Items);

            var options = new SessionCreateOptions
            {
                LineItems = items,
                Mode = "payment",
                CustomerEmail = info.Email,
                //PaymentIntentData = new SessionPaymentIntentDataOptions() {  ReceiptEmail = info.Email },
                SuccessUrl = $"{this.ControllerContext.Request.Headers.Referrer.ToString()}bookingsuccess",
                CancelUrl = $"{this.ControllerContext.Request.Headers.Referrer.ToString()}events",
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Ok(session.Url);


            //var from = new EmailAddress("testbookingrequest@sheffieldcitykayakclub.co.uk", "Booking Request");
            //var to = new EmailAddress("testbookingrequest@sheffieldcitykayakclub.co.uk", "Booking Request");
            //var subject = $"Booking request for {info.Event} on {info.Date}";
            //var htmlContent = $"From: {info.Name}<br/>Email: <a href=\"mailto:{info.Email}\">{info.Email}</a><br/>Number of people: {info.Number}<br/>People: {info.Names}<br/>TelNo: {info.TelNo}<br/>Message:<br/>{info.Message}";
            //var msg = MailHelper.CreateSingleEmail(from, to, subject, htmlContent, htmlContent);
            //msg.ReplyTo = new EmailAddress(info.Email);

            //return await SendMail(msg);
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
    }
}