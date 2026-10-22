using sckc.api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace sckc.api.Extensions
{
    // The review email sent when a trip report is submitted, and the small pages its Accept / Reject links open.
    // Everything a submitter typed is HTML-encoded here; report bodies are already encoded by ToParagraphs.
    public static class TripReportMail
    {
        private const string Red = "#bd0022";
        private const string Ink = "#1f1415";
        private const string Muted = "#6a5759";
        private const string Line = "#8f7c7e";
        private const string Hairline = "#e7dede";
        private const string Ground = "#fbf8f7";
        private const string Font = "Barlow, 'Segoe UI', Helvetica, Arial, sans-serif";

        // Plain text from the form → encoded paragraphs, blank lines between paragraphs, single line breaks kept
        public static string ToParagraphs(string text)
        {
            var paragraphs = Regex.Split((text ?? string.Empty).Replace("\r\n", "\n").Trim(), @"\n\s*\n")
                .Select(p => p.Trim())
                .Where(p => p.Length > 0);

            return string.Concat(paragraphs.Select(p => "<p>" + WebUtility.HtmlEncode(p).Replace("\n", "<br/>") + "</p>"));
        }

        public static string ReviewUrl(string baseUrl, PendingReport pending, string decision)
        {
            return $"{baseUrl}api/reports/review/{Uri.EscapeDataString(pending.id)}?decision={decision}&token={Uri.EscapeDataString(pending.token)}";
        }

        public static string Subject(PendingReport pending) => $"Trip report to review: {pending.report.title}";

        public static string EmailHtml(PendingReport pending, string baseUrl)
        {
            var r = pending.report;
            return $@"<!doctype html>
<html><body style=""margin:0;padding:24px 12px;background:{Ground};font-family:{Font};color:{Ink};"">
<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0""><tr><td align=""center"">
<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:640px;background:#ffffff;border:1px solid {Hairline};border-radius:8px;"">
<tr><td style=""padding:24px;"">
  <p style=""margin:0 0 4px;font-size:13px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:{Red};"">Trip report waiting for review</p>
  <h1 style=""margin:0 0 8px;font-size:26px;line-height:32px;"">{E(r.title)}</h1>
  <p style=""margin:0 0 20px;font-size:14px;color:{Muted};"">Written by {E(r.author)} (<a href=""mailto:{E(pending.submitterEmail)}"" style=""color:{Red};"">{E(pending.submitterEmail)}</a>), submitted {pending.submitted:d MMMM yyyy 'at' HH:mm} UTC</p>
  {Buttons(pending, baseUrl)}
  {FactsTable(r)}
  <div style=""font-size:16px;line-height:24px;"">{r.report}</div>
  <hr style=""border:0;border-top:1px solid {Hairline};margin:24px 0;"">
  {Buttons(pending, baseUrl)}
  <p style=""margin:16px 0 0;font-size:13px;line-height:20px;color:{Muted};"">Each button opens a page where you confirm the choice. Nothing is published until you accept it, and the links stop working once the report has been accepted or rejected. Reply to this email to contact the writer.</p>
</td></tr></table>
</td></tr></table>
</body></html>";
        }

        public static string EmailText(PendingReport pending, string baseUrl)
        {
            var r = pending.report;
            var text = new StringBuilder();
            text.AppendLine("Trip report waiting for review");
            text.AppendLine();
            text.AppendLine(r.title);
            text.AppendLine($"Written by {r.author} ({pending.submitterEmail})");
            text.AppendLine();
            foreach (var (label, value) in Facts(r))
            {
                text.AppendLine($"{label}: {value}");
            }
            text.AppendLine();
            text.AppendLine(WebUtility.HtmlDecode(Regex.Replace(r.report.Replace("</p>", "\n\n").Replace("<br/>", "\n"), "<[^>]+>", string.Empty)).Trim());
            text.AppendLine();
            text.AppendLine($"Accept and publish: {ReviewUrl(baseUrl, pending, "accept")}");
            text.AppendLine($"Reject: {ReviewUrl(baseUrl, pending, "reject")}");
            return text.ToString();
        }

        // The page an email button opens: shows the report and asks for the click that actually changes anything
        public static string ConfirmPage(PendingReport pending, string decision)
        {
            var r = pending.report;
            var accept = decision == "accept";
            var other = accept ? "reject" : "accept";
            var otherLink = $"?decision={other}&token={Uri.EscapeDataString(pending.token)}";

            return Page(accept ? "Publish this trip report?" : "Reject this trip report?", $@"
  <p class=""eyebrow"">{(accept ? "Accept and publish" : "Reject")}</p>
  <h1>{E(r.title)}</h1>
  <p class=""muted"">Written by {E(r.author)} · {r.date:d MMMM yyyy}</p>
  <form method=""post"" action=""{E(Uri.EscapeDataString(pending.id))}"">
    <input type=""hidden"" name=""token"" value=""{E(pending.token)}"">
    <input type=""hidden"" name=""decision"" value=""{decision}"">
    <button type=""submit"" class=""{(accept ? "primary" : "secondary")}"">{(accept ? "Publish report" : "Reject and delete report")}</button>
    <a href=""{E(otherLink)}"">{(accept ? "Reject it instead" : "Publish it instead")}</a>
  </form>
  <p class=""muted"">{(accept ? "The report will appear on the trip reports page straight away." : "The report will be deleted and cannot be recovered.")}</p>
  {FactsTable(r)}
  <div class=""report"">{r.report}</div>");
        }

        public static string DonePage(Report report, string decision, string baseUrl)
        {
            return decision == "accept"
                ? Page("Trip report published", $@"
  <p class=""eyebrow"">Published</p>
  <h1>{E(report.title)}</h1>
  <p>The report is now on the <a href=""{E(baseUrl)}reports"">trip reports page</a>.</p>")
                : Page("Trip report rejected", $@"
  <p class=""eyebrow"">Rejected</p>
  <h1>{E(report.title)}</h1>
  <p>The report has been deleted and will not be published.</p>");
        }

        public static string InvalidLinkPage()
        {
            return Page("Link no longer valid", @"
  <p class=""eyebrow"">Nothing to review</p>
  <h1>This link is no longer valid</h1>
  <p>The report has probably already been accepted or rejected, perhaps by someone else who got the same email. If you think this is wrong, check the trip reports page.</p>");
        }

        private static string Buttons(PendingReport pending, string baseUrl)
        {
            return $@"<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""margin:0 0 20px;""><tr>
    <td style=""border-radius:8px;background:{Red};""><a href=""{E(ReviewUrl(baseUrl, pending, "accept"))}"" style=""display:inline-block;padding:10px 20px;font-size:16px;font-weight:600;color:#ffffff;text-decoration:none;border-radius:8px;"">Accept and publish</a></td>
    <td style=""width:12px;""></td>
    <td style=""border-radius:8px;border:1px solid {Line};""><a href=""{E(ReviewUrl(baseUrl, pending, "reject"))}"" style=""display:inline-block;padding:9px 20px;font-size:16px;font-weight:600;color:{Ink};text-decoration:none;border-radius:8px;"">Reject</a></td>
  </tr></table>";
        }

        private static IEnumerable<(string label, string value)> Facts(Report r)
        {
            var facts = new (string, string)[]
            {
                ("Date", r.date.ToString("d MMMM yyyy")),
                ("Trip", r.trip),
                ("Type", r.triptype),
                ("Start", r.start),
                ("Length", r.triplength),
                ("Organiser", r.organiser),
                ("Coaches", r.coaches),
                ("Paddlers", r.paddlers),
                ("Weather", r.weather),
                ("Phrase of the day", r.phrase),
            };
            return facts.Where(f => !string.IsNullOrWhiteSpace(f.Item2));
        }

        private static string FactsTable(Report r)
        {
            var rows = Facts(r).Select(f =>
                $@"<tr><td style=""padding:4px 16px 4px 0;font-size:13px;font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:{Muted};vertical-align:top;white-space:nowrap;"">{E(f.label)}</td><td style=""padding:4px 0;font-size:15px;"">{E(f.value)}</td></tr>");
            return $@"<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""margin:0 0 20px;padding:12px 16px;background:{Ground};border:1px solid {Hairline};border-radius:8px;width:100%;"">{string.Concat(rows)}</table>";
        }

        private static string Page(string title, string body)
        {
            return $@"<!doctype html>
<html lang=""en""><head><meta charset=""utf-8""><meta name=""viewport"" content=""width=device-width, initial-scale=1""><meta name=""robots"" content=""noindex"">
<title>{E(title)} · Sheffield City Kayak Club</title>
<style>
  body {{ margin:0; padding:32px 16px; background:{Ground}; color:{Ink}; font:16px/24px {Font}; }}
  main {{ max-width:640px; margin:0 auto; padding:24px; background:#fff; border:1px solid {Hairline}; border-radius:8px; }}
  h1 {{ margin:0 0 8px; font-size:26px; line-height:32px; }}
  .eyebrow {{ margin:0 0 4px; font-size:13px; font-weight:700; letter-spacing:.08em; text-transform:uppercase; color:{Red}; }}
  .muted {{ color:{Muted}; font-size:14px; }}
  a {{ color:{Red}; }}
  form {{ display:flex; flex-wrap:wrap; align-items:center; gap:16px; margin:20px 0 8px; }}
  button {{ min-height:40px; padding:0 20px; border-radius:8px; font:600 16px/24px {Font}; cursor:pointer; }}
  button.primary {{ background:{Red}; color:#fff; border:1px solid {Red}; }}
  button.secondary {{ background:#fff; color:{Ink}; border:1px solid {Line}; }}
  button:focus-visible, a:focus-visible {{ outline:2px solid {Ink}; outline-offset:2px; }}
  .report {{ margin-top:16px; }}
</style></head>
<body><main>{body}
</main></body></html>";
        }

        private static string E(string value) => WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
