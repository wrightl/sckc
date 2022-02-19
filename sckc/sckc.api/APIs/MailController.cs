using System;
using System.Net.Mail;
using System.Web.Http;

namespace sckc.api.APIs
{
    public class MailController : ApiController
    {
        [Route("api/ContactUs")]
        [HttpPost]
        public IHttpActionResult SendContactUsMail(ContactUsEmailDto info)
        {
            string to = "enquiries@sheffieldcitykayakclub.co.uk";
            string from = info.Email;
            MailMessage message = new MailMessage(from, to);
            message.Subject = "Contact Us Enquiry";
            message.Body = string.Format("From: {2}\r\nEmail: {3}\r\nSubject: {0}\r\nMessage:\r\n{1}", info.Subject, info.Message, info.Name, info.Email);
            SmtpClient client = new SmtpClient("mail.sheffieldcitykayakclub.co.uk");

            // Credentials are necessary if the server requires the client
            // to authenticate before it will send email on the client's behalf.
            client.UseDefaultCredentials = true;

            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in CreateTestMessage2(): {0}",
                    ex.ToString());
                throw;
            }

            return Ok();
        }

    }

    public class ContactUsEmailDto
    {
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}