using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace sckc.api.Tests
{
    // Runs the real middleware pipeline from Program.cs in memory, against a temporary copy of the deployed folder layout
    [TestClass]
    public class PipelineTests
    {
        // Secrets, binaries and config are deployed to the same folder as the public site, so they must never be served
        private static readonly string[] PrivateFiles =
        [
            "data/sckc.data",
            "data/tripreports.json",
            "Resources/pool_booking_request_response.html",
            "bin/readme.txt",
            "logs/app.txt",
            "sckc.api.dll",
            "sckc.api.pdb",
            "sckc.api.exe",
            "web.config",
            "sckc.api.deps.json",
            "sckc.api.runtimeconfig.json",
            "appsettings.json",
            "app/appsettings.Production.json",
        ];

        private const string SecretMarker = "sckc-secret-marker";

        private static string root;
        private static WebApplicationFactory<Program> factory;

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            root = Path.Combine(Path.GetTempPath(), "sckc-site-" + Guid.NewGuid().ToString("N"));
            foreach (var file in PrivateFiles)
            {
                Write(file, $"{{\"Marker\": \"{SecretMarker}\"}}");
            }
            Write("index.html", "<html>site</html>");
            Write("app/index.html", "<html>staged</html>");
            Write("gallery/2025-06-01@Trip/a.jpg", "jpg");

            factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseContentRoot(root));
        }

        [ClassCleanup]
        public static void Teardown()
        {
            factory.Dispose();
            Directory.Delete(root, recursive: true);
        }

        private static void Write(string relativePath, string content)
        {
            var path = Path.Combine(root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content);
        }

        [TestMethod]
        public async Task Health_ReturnsHealthyAsJson()
        {
            var response = await factory.CreateClient().GetAsync("/api/Health");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json", response.Content.Headers.ContentType?.MediaType);
            Assert.AreEqual("\"Healthy\"", await response.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task UnknownApiRoute_ReturnsNotFound()
        {
            var response = await factory.CreateClient().GetAsync("/api/DoesNotExist");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task UnhandledError_ReturnsProblemDetailsAndIsLogged()
        {
            Write("data/tripreports.data", "{ not json");
            try
            {
                using var production = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production"));
                var response = await production.CreateClient().GetAsync("/api/Reports");
                var body = await response.Content.ReadAsStringAsync();

                Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);
                StringAssert.Contains(body, "tripreports.data is not valid JSON");
                StringAssert.Contains(body, "InvalidDataException");
                StringAssert.Contains(File.ReadAllText(Path.Combine(root, "logs", "errors.log")), "GET /api/Reports");
            }
            finally
            {
                File.Delete(Path.Combine(root, "data", "tripreports.data"));
            }
        }

        [TestMethod]
        [DataRow("/", "<html>site</html>")]
        [DataRow("/events", "<html>site</html>")]
        [DataRow("/app/", "<html>staged</html>")]
        [DataRow("/app/events", "<html>staged</html>")]
        [DataRow("/gallery/2025-06-01@Trip/a.jpg", "jpg")]
        public async Task PublicFilesAndAngularRoutes_AreServed(string path, string expected)
        {
            var response = await factory.CreateClient().GetAsync(path);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(expected, await response.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task PrivateFiles_AreNotServed()
        {
            var client = factory.CreateClient();
            var served = new List<string>();

            foreach (var file in PrivateFiles)
            {
                foreach (var path in new[] { "/" + file, "/" + file.ToUpperInvariant() })
                {
                    var response = await client.GetAsync(path);
                    var body = await response.Content.ReadAsStringAsync();

                    if (body.Contains(SecretMarker))
                    {
                        served.Add(path);
                    }
                }
            }

            Assert.AreEqual(0, served.Count, "Served: " + string.Join(", ", served));
        }
    }
}
