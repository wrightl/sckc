using sckc.api.Extensions;
using sckc.api.Models;
using sckc.core.Models;
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
        public async Task<IHttpActionResult> SendContactUsMail(ContactUsDto info)
        {
            var from = new EmailAddress("contactus@sheffieldcitykayakclub.co.uk", "Contact Us");
            var to = new EmailAddress("contactus@sheffieldcitykayakclub.co.uk", "Contact Us");
            var subject = "Contact Us Enquiry";
            var htmlContent = string.Format("From: {2}<br/>Email: <a href=\"mailto:{3}\">{3}</a><br/>Subject: {0}<br/>Message:<br/>{1}", info.Subject, info.Message, info.Name, info.Email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, htmlContent, htmlContent);
            msg.ReplyTo = new EmailAddress(info.Email);

            var response = await Helper.SendMail(msg);

            if (response)
                return Ok("Thanks for getting in touch. Someone will reply to your email soon");
            return InternalServerError();
        }

        [Route("api/BookingEnquiry")]
        [HttpPost]
        public async Task<IHttpActionResult> SendBookingEnquiry(BookingRequestDto info)
        {
            var from = new EmailAddress("bookingrequest@sheffieldcitykayakclub.co.uk", "Booking Request");
            var to = new EmailAddress("bookingrequest@sheffieldcitykayakclub.co.uk", "Booking Request");
            var subject = $"Booking request for {info.Event} on {info.Date}";
            var htmlContent = $"From: {info.Name}<br/>Email: <a href=\"mailto:{info.Email}\">{info.Email}</a><br/>Number of people: {info.Number}<br/>People: {info.Names}<br/>TelNo: {info.TelNo}<br/>Message:<br/>{info.Message}";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, htmlContent, htmlContent);
            msg.ReplyTo = new EmailAddress(info.Email);

            var response = await Helper.SendMail(msg);

            if (response)
            {
                // Send template email
                to = new EmailAddress(info.Email);
                subject = $"Booking request for {info.Event} on {info.Date}";

                htmlContent = $"Here's some info....";
                msg = MailHelper.CreateSingleEmail(from, to, subject, htmlContent, htmlContent);

                response = await Helper.SendMail(msg);

                return Ok("Thanks for getting in touch. Someone will reply to your email soon");
            }
            return InternalServerError();
        }
    }
}