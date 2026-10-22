using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sckc.api.Extensions;
using sckc.api.Models;

namespace sckc.api.Tests
{
    // The request Helper.SendWithResend makes, checked against a fake Resend so nothing is sent
    [TestClass]
    public class ResendTests
    {
        private string originalRoot;
        private HttpClient originalClient;
        private string root;
        private FakeResend resend;

        [TestInitialize]
        public void Setup()
        {
            originalRoot = Helper.ContentRootPath;
            originalClient = Helper.ResendClient;
            root = Path.Combine(Path.GetTempPath(), "sckc-resend-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "data"));
            File.WriteAllText(Path.Combine(root, "data", "resend_apikey.data"), "re_test_key\n");
            Helper.ContentRootPath = root;
            resend = new FakeResend();
            Helper.ResendClient = new HttpClient(resend);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Helper.ContentRootPath = originalRoot;
            Helper.ResendClient = originalClient;
            Directory.Delete(root, recursive: true);
        }

        private static EmailMessage Message() => new(
            new EmailAddress("contactus@sheffieldcitykayakclub.co.uk", "Trip Reports"),
            new EmailAddress("admin@sheffieldcitykayakclub.co.uk"),
            "Trip report to review",
            "<p>Hello</p>",
            "Hello")
        {
            ReplyTo = new EmailAddress("lee@example.com", "Lee"),
        };

        [TestMethod]
        public async Task Send_PostsTheEmailToResend()
        {
            Assert.IsTrue(await Helper.SendWithResend(Message()));

            Assert.AreEqual(HttpMethod.Post, resend.Request.Method);
            Assert.AreEqual("https://api.resend.com/emails", resend.Request.RequestUri.ToString());
            Assert.AreEqual("Bearer re_test_key", resend.Request.Headers.Authorization.ToString());

            var body = JsonDocument.Parse(resend.Body).RootElement;
            Assert.AreEqual("Trip Reports <contactus@sheffieldcitykayakclub.co.uk>", body.GetProperty("from").GetString());
            Assert.AreEqual("admin@sheffieldcitykayakclub.co.uk", body.GetProperty("to")[0].GetString());
            Assert.AreEqual("Trip report to review", body.GetProperty("subject").GetString());
            Assert.AreEqual("<p>Hello</p>", body.GetProperty("html").GetString());
            Assert.AreEqual("Hello", body.GetProperty("text").GetString());
            Assert.AreEqual("Lee <lee@example.com>", body.GetProperty("reply_to").GetString());
        }

        [TestMethod]
        public async Task Send_LeavesOutTextAndReplyToWhenNotSet()
        {
            await Helper.SendWithResend(new EmailMessage(new EmailAddress("a@example.com"), new EmailAddress("b@example.com"), "Hi", "<p>Hi</p>"));

            var body = JsonDocument.Parse(resend.Body).RootElement;
            Assert.IsFalse(body.TryGetProperty("text", out _), "an empty text would stop Resend generating one");
            Assert.IsFalse(body.TryGetProperty("reply_to", out _));
        }

        [TestMethod]
        public async Task Send_ThrowsWithResendsMessageWhenRefused()
        {
            resend.Status = HttpStatusCode.Forbidden;
            resend.Response = "{\"statusCode\":403,\"name\":\"validation_error\",\"message\":\"The sheffieldcitykayakclub.co.uk domain is not verified.\"}";

            var ex = await Assert.ThrowsExactlyAsync<Exception>(() => Helper.SendWithResend(Message()));

            StringAssert.Contains(ex.Message, "(403)");
            StringAssert.Contains(ex.Message, "domain is not verified");
        }

        [TestMethod]
        public async Task Send_ExplainsAMissingApiKey()
        {
            File.Delete(Path.Combine(root, "data", "resend_apikey.data"));

            var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => Helper.SendWithResend(Message()));

            StringAssert.Contains(ex.Message, "resend_apikey.data is missing");
            Assert.IsNull(resend.Request);
        }

        private class FakeResend : HttpMessageHandler
        {
            public HttpRequestMessage Request;
            public string Body;
            public HttpStatusCode Status = HttpStatusCode.OK;
            public string Response = "{\"id\":\"49a3999c-0ce1-4ea6-ab68-afcd6dc2e794\"}";

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Request = request;
                Body = await request.Content.ReadAsStringAsync(cancellationToken);
                return new HttpResponseMessage(Status) { Content = new StringContent(Response) };
            }
        }
    }
}
