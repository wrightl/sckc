using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using sckc.api.APIs;
using sckc.api.Extensions;
using sckc.api.Models;

namespace sckc.api.Tests
{
    // Submitting a trip report, the review email, and accepting or rejecting it from the email's links
    [TestClass]
    public class TripReportSubmissionTests
    {
        private const string ExistingReports =
            "{\"items\":[{\"date\":\"2014/08/30\",\"title\":\"Matlock\",\"report\":\"<p>Old</p>\",\"id\":\"0\",\"published\":true}]}";

        private string originalRoot;
        private Func<EmailMessage, Task<bool>> originalSender;
        private string root;
        private List<EmailMessage> sent;

        [TestInitialize]
        public void Setup()
        {
            originalRoot = Helper.ContentRootPath;
            originalSender = Helper.MailSender;
            root = Path.Combine(Path.GetTempPath(), "sckc-reports-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "data"));
            File.WriteAllText(Path.Combine(root, "data", "tripreports.data"), ExistingReports);
            Helper.ContentRootPath = root;
            sent = new List<EmailMessage>();
            Helper.MailSender = message => { sent.Add(message); return Task.FromResult(true); };
        }

        [TestCleanup]
        public void Cleanup()
        {
            Helper.ContentRootPath = originalRoot;
            Helper.MailSender = originalSender;
            Directory.Delete(root, recursive: true);
        }

        private static TripReportSubmissionDto Submission(string report = null) => new()
        {
            title = "River Wye, Glasbury to Hay",
            date = "2026-10-18",
            trip = "River Wye",
            triptype = "Kayak",
            start = "Glasbury",
            paddlers = "Angel, Lee & Steve",
            report = report ?? "We launched at Glasbury at ten.\n\nThe river was low but lovely, and nobody swam.",
            author = "Lee",
            email = "lee@example.com",
        };

        private async Task<PendingReport> SubmitAsync(TripReportSubmissionDto submission = null)
        {
            var result = await new ReportsController().Submit(submission ?? Submission());
            Assert.IsInstanceOfType<OkObjectResult>(result);
            return JArray.Parse(File.ReadAllText(Path.Combine(root, "data", "pendingreports.data"))).Last.ToObject<PendingReport>();
        }

        private List<Report> Published() => (List<Report>)((OkObjectResult)new ReportsController().Get()).Value;

        private static string Html(IActionResult result) => ((ContentResult)result).Content;

        private static int? Status(IActionResult result) => ((ContentResult)result).StatusCode;

        [TestMethod]
        public async Task Submit_KeepsReportPendingAndEmailsItForReview()
        {
            var pending = await SubmitAsync();

            CollectionAssert.AreEqual(new[] { "Matlock" }, Published().Select(r => r.title).ToArray());
            Assert.IsFalse(pending.report.published);
            Assert.AreEqual("<p>We launched at Glasbury at ten.</p><p>The river was low but lovely, and nobody swam.</p>", pending.report.report);

            var email = sent.Single();
            var html = email.Html;
            Assert.AreEqual("Trip report to review: River Wye, Glasbury to Hay", email.Subject);
            Assert.AreEqual(Constants.TripReportsEmailAddress, email.To.Single().Email);
            Assert.AreEqual(Constants.TripReportsFromAddress, email.From.Email);
            Assert.AreEqual("lee@example.com", email.ReplyTo.Email);
            StringAssert.Contains(html, "nobody swam");
            StringAssert.Contains(html, $"api/reports/review/{pending.id}?decision=accept&amp;token={pending.token}");
            StringAssert.Contains(html, $"api/reports/review/{pending.id}?decision=reject&amp;token={pending.token}");
        }

        [TestMethod]
        public async Task Submit_EncodesMarkupInTheReport()
        {
            var pending = await SubmitAsync(Submission("<script>alert('x')</script> and a long enough report to pass validation."));

            Assert.IsFalse(pending.report.report.Contains("<script>"));
            StringAssert.Contains(pending.report.report, "&lt;script&gt;");
            Assert.IsFalse(sent.Single().Html.Contains("<script>"));
        }

        [TestMethod]
        public async Task Submit_FromSpamBotIsIgnored()
        {
            var submission = Submission();
            submission.website = "http://spam.example";

            Assert.IsInstanceOfType<OkObjectResult>(await new ReportsController().Submit(submission));
            Assert.AreEqual(0, sent.Count);
            Assert.IsFalse(File.Exists(Path.Combine(root, "data", "pendingreports.data")));
        }

        [TestMethod]
        public async Task Submit_DiscardsReportWhenEmailFails()
        {
            Helper.MailSender = _ => throw new Exception("Resend is down");

            await Assert.ThrowsExactlyAsync<Exception>(() => new ReportsController().Submit(Submission()));
            Assert.AreEqual("[]", File.ReadAllText(Path.Combine(root, "data", "pendingreports.data")));
        }

        [TestMethod]
        public async Task ConfirmPage_ShowsReportWithoutChangingAnything()
        {
            var pending = await SubmitAsync();

            var page = new ReportsController().ConfirmReview(pending.id, "accept", pending.token);

            StringAssert.Contains(Html(page), "Publish report");
            StringAssert.Contains(Html(page), "nobody swam");
            Assert.IsNotNull(TripReportStore.FindPending(pending.id, pending.token));
            Assert.AreEqual(1, Published().Count);
        }

        [TestMethod]
        public async Task Accept_PublishesReportAndKeepsExistingOnes()
        {
            var pending = await SubmitAsync();

            var result = new ReportsController().Review(pending.id, "accept", pending.token);

            StringAssert.Contains(Html(result), "Published");
            CollectionAssert.AreEqual(new[] { "River Wye, Glasbury to Hay", "Matlock" }, Published().Select(r => r.title).ToArray());

            // The file keeps its "items" wrapper, its existing entry untouched, and the yyyy/MM/dd date style
            var file = JObject.Parse(File.ReadAllText(Path.Combine(root, "data", "tripreports.data")));
            var items = (JArray)file["items"];
            Assert.AreEqual("2014/08/30", (string)items[0]["date"]);
            Assert.AreEqual("2026/10/18", (string)items[1]["date"]);
            Assert.IsTrue((bool)items[1]["published"]);
            Assert.IsNull(items[1]["token"]);
            Assert.IsNull(TripReportStore.FindPending(pending.id, pending.token));
        }

        [TestMethod]
        public async Task Reject_DeletesReportWithoutPublishing()
        {
            var pending = await SubmitAsync();

            var result = new ReportsController().Review(pending.id, "reject", pending.token);

            StringAssert.Contains(Html(result), "Rejected");
            Assert.AreEqual(1, Published().Count);
            Assert.IsNull(TripReportStore.FindPending(pending.id, pending.token));
        }

        [TestMethod]
        public async Task Review_RefusesWrongTokenAndUsedLinks()
        {
            var pending = await SubmitAsync();
            var controller = new ReportsController();

            Assert.AreEqual(404, Status(controller.Review(pending.id, "accept", "not-the-token")));
            Assert.AreEqual(404, Status(controller.ConfirmReview(pending.id, "accept", null)));
            Assert.AreEqual(404, Status(controller.Review(pending.id, "publish", pending.token)));
            Assert.AreEqual(1, Published().Count);

            Assert.AreEqual(200, Status(controller.Review(pending.id, "reject", pending.token)));
            Assert.AreEqual(404, Status(controller.Review(pending.id, "accept", pending.token)));
            StringAssert.Contains(Html(controller.ConfirmReview(pending.id, "accept", pending.token)), "no longer valid");
            Assert.AreEqual(1, Published().Count);
        }

        // The whole journey through the real pipeline: JSON submit, the email's GET link, then the confirm page's form POST
        [TestMethod]
        public async Task Pipeline_SubmitThenAcceptFromEmailLink()
        {
            Write("index.html", "<html>site</html>");
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseContentRoot(root));
            Helper.MailSender = message => { sent.Add(message); return Task.FromResult(true); };
            var client = factory.CreateClient();

            var invalid = await client.PostAsJsonAsync("/api/reports", new { title = "No date", trip = "Don", report = "Too short", author = "Lee", email = "lee@example.com" });
            Assert.AreEqual(HttpStatusCode.BadRequest, invalid.StatusCode);
            Assert.AreEqual(0, sent.Count);

            var submit = await client.PostAsJsonAsync("/api/reports", Submission());
            Assert.AreEqual(HttpStatusCode.OK, submit.StatusCode);
            StringAssert.Contains(await submit.Content.ReadAsStringAsync(), "once it has been checked");

            var text = sent.Single().Text;
            var acceptLink = Regex.Match(text, @"Accept and publish: (\S+)").Groups[1].Value;
            StringAssert.StartsWith(acceptLink, "http://localhost/api/reports/review/");

            var confirm = await client.GetAsync(new Uri(acceptLink).PathAndQuery);
            var confirmHtml = await confirm.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, confirm.StatusCode);
            Assert.AreEqual("text/html", confirm.Content.Headers.ContentType?.MediaType);
            Assert.AreEqual(1, Published().Count, "opening the link must not publish anything");

            var token = Regex.Match(confirmHtml, "name=\"token\" value=\"([^\"]+)\"").Groups[1].Value;
            var id = new Uri(acceptLink).Segments.Last();
            var post = await client.PostAsync($"/api/reports/review/{id}",
                new FormUrlEncodedContent(new Dictionary<string, string> { ["token"] = token, ["decision"] = "accept" }));

            Assert.AreEqual(HttpStatusCode.OK, post.StatusCode);
            StringAssert.Contains(await post.Content.ReadAsStringAsync(), "trip reports page");
            var reports = await client.GetFromJsonAsync<List<Report>>("/api/reports");
            CollectionAssert.AreEqual(new[] { "River Wye, Glasbury to Hay", "Matlock" }, reports.Select(r => r.title).ToArray());
        }

        private void Write(string relativePath, string content)
        {
            var path = Path.Combine(root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content);
        }
    }
}
