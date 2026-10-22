using sckc.api.Extensions;
using sckc.api.Models;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace sckc.api.APIs
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        public const string SubmittedMessage = "Thanks for your trip report. It will appear on the trip reports page once it has been checked.";

        // GET api/<controller>
        // Always returns a list of published reports (newest first); an empty list means none have been published yet
        [HttpGet]
        public IActionResult Get()
        {
            var reports = TripReportStore.ReadPublished();

            reports.Sort((Report x, Report y) => y.date.CompareTo(x.date));

            return Ok(reports);
        }

        // POST api/<controller>
        // Saves the report as pending and emails it for review; nothing is published until it is accepted
        [HttpPost]
        public async Task<IActionResult> Submit(TripReportSubmissionDto submission)
        {
            // Spam bots fill in every field, including the hidden one; let them think it worked
            if (!string.IsNullOrEmpty(submission.website))
            {
                return Ok(SubmittedMessage);
            }

            if (!DateTime.TryParseExact(submission.date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                ModelState.AddModelError(nameof(submission.date), "Enter the date of the trip.");
            }
            else if (date > DateTime.Today.AddDays(1))
            {
                ModelState.AddModelError(nameof(submission.date), "The trip date can't be in the future.");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var report = new Report
            {
                id = Guid.NewGuid().ToString("N"),
                date = date,
                title = submission.title.Trim(),
                trip = submission.trip.Trim(),
                triptype = submission.triptype?.Trim(),
                triplength = submission.triplength?.Trim(),
                start = submission.start?.Trim(),
                organiser = submission.organiser?.Trim(),
                coaches = submission.coaches?.Trim(),
                paddlers = submission.paddlers?.Trim(),
                weather = submission.weather?.Trim(),
                phrase = submission.phrase?.Trim(),
                report = TripReportMail.ToParagraphs(submission.report),
                author = submission.author.Trim(),
                photos = string.Empty,
                published = false,
            };

            var pending = TripReportStore.AddPending(report, submission.email.Trim());

            try
            {
                await Helper.SendMail(BuildReviewEmail(pending));
            }
            catch
            {
                // Nobody would ever hear about it, so don't keep it; the global handler reports the failure
                TripReportStore.RemovePending(pending.id);
                throw;
            }

            return Ok(SubmittedMessage);
        }

        // GET api/reports/review/{id}?decision=accept|reject&token=...
        // Opened from the review email. Only shows a confirmation page: email security scanners open links on their
        // own, so the change happens on the page's button (a POST), never on the link itself.
        [HttpGet("review/{id}")]
        public IActionResult ConfirmReview(string id, [FromQuery] string decision, [FromQuery] string token)
        {
            var pending = TripReportStore.FindPending(id, token);
            if (pending == null || !IsDecision(decision))
            {
                return Html(TripReportMail.InvalidLinkPage(), 404);
            }

            return Html(TripReportMail.ConfirmPage(pending, decision));
        }

        // POST api/reports/review/{id}, from the confirmation page's button
        [HttpPost("review/{id}")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult Review(string id, [FromForm] string decision, [FromForm] string token)
        {
            if (!IsDecision(decision))
            {
                return Html(TripReportMail.InvalidLinkPage(), 404);
            }

            var report = decision == "accept" ? TripReportStore.Accept(id, token) : TripReportStore.Reject(id, token);
            if (report == null)
            {
                return Html(TripReportMail.InvalidLinkPage(), 404);
            }

            return Html(TripReportMail.DonePage(report, decision, SiteBaseUrl()));
        }

        private EmailMessage BuildReviewEmail(PendingReport pending)
        {
            var baseUrl = SiteBaseUrl();
            var from = new EmailAddress(Constants.TripReportsFromAddress, Constants.TripReportsEmailName);
            var reviewers = new EmailAddress(Constants.TripReportsEmailAddress, Constants.TripReportsEmailName);
            // Resend's click tracking (set per domain) must stay off, or it would rewrite the links and see the token
            return new EmailMessage(from, reviewers, TripReportMail.Subject(pending),
                TripReportMail.EmailHtml(pending, baseUrl), TripReportMail.EmailText(pending, baseUrl))
            {
                ReplyTo = new EmailAddress(pending.submitterEmail, pending.report.author),
            };
        }

        // Production links always point at the live site. When running locally they point at this API instead,
        // so the review links can be tried out without touching the live reports.
        private string SiteBaseUrl()
        {
            var environment = HttpContext?.RequestServices.GetService<IWebHostEnvironment>();
            if (environment?.IsDevelopment() == true)
            {
                return $"{Request.Scheme}://{Request.Host}/";
            }

            return Constants.SiteBaseUrl;
        }

        private static bool IsDecision(string decision) => decision is "accept" or "reject";

        private ContentResult Html(string html, int status = 200)
        {
            return new ContentResult { Content = html, ContentType = "text/html; charset=utf-8", StatusCode = status };
        }
    }
}
