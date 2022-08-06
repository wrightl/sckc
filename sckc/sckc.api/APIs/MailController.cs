using sckc.api.Extensions;
using sckc.api.Models;
using SendGrid.Helpers.Mail;
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
            var from = new EmailAddress(Constants.ContactUsEmailAddress, Constants.ContactUsEmailAddress);
            var to = new EmailAddress(Constants.ContactUsEmailAddress, Constants.ContactUsEmailAddress);
            var subject = "Contact Us Enquiry";
            var htmlContent = string.Format("From: {2}<br/>Email: <a href=\"mailto:{3}\">{3}</a><br/>Subject: {0}<br/>Message:<br/>{1}", info.Subject, info.Message, info.Name, info.Email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, htmlContent, htmlContent);
            msg.ReplyTo = new EmailAddress(info.Email);

            var response = await Helper.SendMail(msg);

            if (response)
                return Ok("Thanks for getting in touch. Someone will reply to your email soon");
            return InternalServerError();
        }
    }
}