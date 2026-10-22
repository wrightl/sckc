using System.Collections.Generic;

namespace sckc.api.Models
{
    // An email the site sends, independent of the provider that delivers it (Resend, see Helper.SendMail)
    public class EmailMessage
    {
        public EmailMessage(EmailAddress from, EmailAddress to, string subject, string html, string text = null)
        {
            From = from;
            To = new List<EmailAddress> { to };
            Subject = subject;
            Html = html;
            Text = text;
        }

        public EmailAddress From { get; }

        public List<EmailAddress> To { get; }

        public string Subject { get; }

        public string Html { get; }

        // Plain-text version; when null the provider generates one from the HTML
        public string Text { get; }

        public EmailAddress ReplyTo { get; set; }
    }

    public record EmailAddress(string Email, string Name = null)
    {
        // "Name <email>", the form Resend expects
        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? Email : $"{Name.Replace("\"", "'")} <{Email}>";
    }
}
