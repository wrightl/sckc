using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sckc.api.APIs;
using sckc.api.Extensions;
using sckc.api.Models;

namespace sckc.api.Tests
{
    // Controllers that read gallery folders and trip reports from the content root, pointed at a temporary folder
    [TestClass]
    public class ContentControllerTests
    {
        private string originalRoot;
        private string root;

        [TestInitialize]
        public void Setup()
        {
            originalRoot = Helper.ContentRootPath;
            root = Path.Combine(Path.GetTempPath(), "sckc-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            Helper.ContentRootPath = root;
        }

        [TestCleanup]
        public void Cleanup()
        {
            Helper.ContentRootPath = originalRoot;
            Directory.Delete(root, recursive: true);
        }

        private void AddPhotos(string folder, params string[] files)
        {
            var path = Path.Combine(root, "gallery", folder);
            Directory.CreateDirectory(path);
            foreach (var file in files)
            {
                File.WriteAllText(Path.Combine(path, file), string.Empty);
            }
        }

        private static T OkValue<T>(IActionResult result)
        {
            Assert.IsInstanceOfType<OkObjectResult>(result);
            return (T)((OkObjectResult)result).Value;
        }

        [TestMethod]
        public void Health_ReturnsHealthy()
        {
            Assert.AreEqual("Healthy", OkValue<string>(new HealthController().Get()));
        }

        [TestMethod]
        public void Gallery_ListsPhotoSetsNewestFirst()
        {
            AddPhotos("2025-06-01@River_Trip", "b.jpg", "primary.jpg");
            AddPhotos("2026-03-15@Pool_Night", "a.jpg");

            var sets = OkValue<List<PhotoSet>>(new GalleryController().Get());

            CollectionAssert.AreEqual(new[] { "Pool Night", "River Trip" }, sets.Select(s => s.title).ToArray());
            Assert.AreEqual(2, sets[1].photoCount);
            Assert.AreEqual(new DateTime(2025, 6, 1), sets[1].created);
        }

        [TestMethod]
        public void Gallery_PrefersPrimaryPhotoForCover()
        {
            AddPhotos("2025-06-01@Trip", "a.jpg", "primary.jpeg");
            AddPhotos("2025-07-01@Other", "a.jpg");

            var sets = OkValue<List<PhotoSet>>(new GalleryController().Get());

            Assert.AreEqual("gallery/2025-06-01@Trip/primary.jpeg", sets.Single(s => s.title == "Trip").primaryPhotoUrl);
            Assert.AreEqual("gallery/2025-07-01@Other/a.jpg", sets.Single(s => s.title == "Other").primaryPhotoUrl);
        }

        [TestMethod]
        public void Gallery_SkipsEmptyAndBadlyNamedFolders()
        {
            AddPhotos("2025-06-01@Trip", "a.jpg");
            AddPhotos("2025-06-02@Empty");
            AddPhotos("not-a-photo-set", "a.jpg");

            var sets = OkValue<List<PhotoSet>>(new GalleryController().Get());

            Assert.AreEqual("Trip", sets.Single().title);
        }

        [TestMethod]
        public void Gallery_ThrowsWhenGalleryFolderIsMissing()
        {
            Assert.ThrowsExactly<Exception>(() => new GalleryController().Get());
        }

        [TestMethod]
        public void GetPhotoSet_ReturnsPhotoUrls()
        {
            AddPhotos("2025-06-01@River_Trip", "a.jpg", "b.jpg");

            var value = OkValue<object>(new GalleryController().GetPhotoSet("2025-06-01@River_Trip"));
            var photos = ((IEnumerable<string>)value.GetType().GetProperty("Photos").GetValue(value)).OrderBy(p => p).ToArray();

            Assert.AreEqual("River Trip", value.GetType().GetProperty("Title").GetValue(value));
            CollectionAssert.AreEqual(new[] { "gallery/2025-06-01@River_Trip/a.jpg", "gallery/2025-06-01@River_Trip/b.jpg" }, photos);
        }

        [TestMethod]
        public void Reports_ReturnsPublishedReportsNewestFirst()
        {
            Directory.CreateDirectory(Path.Combine(root, "data"));
            File.WriteAllText(Path.Combine(root, "data", "tripreports.data"),
                "[{\"id\":\"1\",\"title\":\"Old\",\"date\":\"2024-01-01\",\"published\":true},{\"id\":\"2\",\"title\":\"New\",\"date\":\"2025-01-01\",\"published\":true},{\"id\":\"3\",\"title\":\"Draft\",\"date\":\"2025-02-01\",\"published\":false}]");

            var reports = OkValue<List<Report>>(new ReportsController().Get());

            CollectionAssert.AreEqual(new[] { "New", "Old" }, reports.Select(r => r.title).ToArray());
        }

        private void WriteReports(string json)
        {
            Directory.CreateDirectory(Path.Combine(root, "data"));
            File.WriteAllText(Path.Combine(root, "data", "tripreports.data"), json);
        }

        [TestMethod]
        public void Reports_ReadsReportsWrappedInItems()
        {
            WriteReports("{\"items\":[{\"id\":\"1\",\"title\":\"Old\",\"date\":\"2014/08/30\",\"published\":true},{\"id\":\"2\",\"title\":\"New\",\"date\":\"2019/02/24\",\"published\":true}]}");

            var reports = OkValue<List<Report>>(new ReportsController().Get());

            CollectionAssert.AreEqual(new[] { "New", "Old" }, reports.Select(r => r.title).ToArray());
        }

        [TestMethod]
        public void Reports_InvalidJsonThrowsWithPosition()
        {
            WriteReports("[{\"id\":\"1\",");

            var ex = Assert.ThrowsExactly<InvalidDataException>(() => new ReportsController().Get());

            StringAssert.Contains(ex.Message, "tripreports.data is not valid JSON (line 1");
        }

        [TestMethod]
        public void Reports_UnexpectedShapeThrowsNamingProperties()
        {
            WriteReports("{\"reports\":[]}");

            var ex = Assert.ThrowsExactly<InvalidDataException>(() => new ReportsController().Get());

            StringAssert.Contains(ex.Message, "found an object with properties: reports");
        }

        [TestMethod]
        public void Reports_BadReportThrowsNamingTheReport()
        {
            WriteReports("[{\"id\":\"1\",\"title\":\"Fine\",\"date\":\"2024-01-01\"},{\"id\":\"2\",\"title\":\"Broken\",\"date\":\"not a date\"}]");

            var ex = Assert.ThrowsExactly<InvalidDataException>(() => new ReportsController().Get());

            StringAssert.Contains(ex.Message, "report 1 (\"Broken\")");
        }

        [TestMethod]
        public void Reports_ReturnsEmptyListWhenReportsAreMissing()
        {
            Assert.AreEqual(0, OkValue<List<Report>>(new ReportsController().Get()).Count);

            Directory.CreateDirectory(Path.Combine(root, "data"));
            Assert.AreEqual(0, OkValue<List<Report>>(new ReportsController().Get()).Count);

            File.WriteAllText(Path.Combine(root, "data", "tripreports.data"), "");
            Assert.AreEqual(0, OkValue<List<Report>>(new ReportsController().Get()).Count);
        }
    }
}
