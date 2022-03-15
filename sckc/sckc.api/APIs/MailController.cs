using sckc.api.Extensions;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Http;

namespace sckc.api.APIs
{
    public class MailController : ApiController
    {
        [Route("api/ContactUs")]
        [HttpPost]
        public async Task<IHttpActionResult> SendContactUsMail(ContactUsEmailDto info)
        {
            var from = new EmailAddress("contactus@sheffieldcitykayakclub.co.uk", "Contact Us");
            var to = new EmailAddress("contactus@sheffieldcitykayakclub.co.uk", "Contact Us");
            var subject = "Contact Us Enquiry";
            var htmlContent = string.Format("From: {2}<br/>Email: <a href=\"mailto:{3}\">{3}</a><br/>Subject: {0}<br/>Message:<br/>{1}", info.Subject, info.Message, info.Name, info.Email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, htmlContent, htmlContent);
            msg.ReplyTo = new EmailAddress(info.Email);

            return await SendMail(msg);
        }

        [Route("api/BookingRequest")]
        [HttpPost]
        public async Task<IHttpActionResult> SendBookingRequestMail(BookingRequestEmailDto info)
        {
            var from = new EmailAddress("testbookingrequest@sheffieldcitykayakclub.co.uk", "Booking Request");
            var to = new EmailAddress("testbookingrequest@sheffieldcitykayakclub.co.uk", "Booking Request");
            var subject = $"Booking request for {info.Event} on {info.Date}";
            var htmlContent = $"From: {info.Name}<br/>Email: <a href=\"mailto:{info.Email}\">{info.Email}</a><br/>People: {info.Names}<br/>TelNo: {info.TelNo}<br/>Message:<br/>{info.Message}";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, htmlContent, htmlContent);
            msg.ReplyTo = new EmailAddress(info.Email);

            return await SendMail(msg);
        }

        private async Task<IHttpActionResult> SendMail(SendGridMessage message)
        {
            var apiKey = File.ReadAllText(Helper.MapPath("data/sendgrid_apikey.data"));
            var client = new SendGridClient(apiKey);
            var response = await client.SendEmailAsync(message);

            if (response?.IsSuccessStatusCode == true)
                return Ok("Thanks for getting in touch. Someone will reply to your email soon");

            throw new Exception(await response.Body.ReadAsStringAsync());
        }
    }

    public class ContactUsEmailDto
    {
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class BookingRequestEmailDto
    {
        public string Names { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string TelNo { get; set; }
        public string Event { get; set; }
        public string Date { get; set; }
    }
}