using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sckc.api.Extensions;
using sckc.core.Models;

namespace sckc.api.Tests
{
    [TestClass]
    public class HelperTests
    {
        private string originalRoot;

        [TestInitialize]
        public void Setup()
        {
            originalRoot = Helper.ContentRootPath;
            Helper.ContentRootPath = Path.Combine(Path.GetTempPath(), "sckc-root");
        }

        [TestCleanup]
        public void Cleanup() => Helper.ContentRootPath = originalRoot;

        [TestMethod]
        public void MapPath_CombinesRelativePathWithContentRoot()
        {
            var expected = Path.Combine(Helper.ContentRootPath, "data", "sckc.data");

            Assert.AreEqual(expected, Helper.MapPath("data/sckc.data"));
        }

        [TestMethod]
        public void MapPath_IgnoresLeadingSlash()
        {
            Assert.AreEqual(Helper.MapPath("gallery/a.jpg"), Helper.MapPath("/gallery/a.jpg"));
        }

        [TestMethod]
        public void ReplaceTemplatePlaceholders_ReplacesBookingFields()
        {
            var info = new BookingRequestDto { Name = "Sam", Event = "Pool session", Date = "01/11/26", Number = 3 };

            var result = Helper.ReplaceTemplatePlaceholders("Hi {{Name}}, {{Number}} places for {{Event}} on {{Date}}", info);

            Assert.AreEqual("Hi Sam, 3 places for Pool session on 01/11/26", result);
        }

        [TestMethod]
        public void ReplaceTemplatePlaceholders_ReplacesUnsetFieldsWithEmptyText()
        {
            var result = Helper.ReplaceTemplatePlaceholders("Message: [{{Message}}]", new BookingRequestDto());

            Assert.AreEqual("Message: []", result);
        }
    }
}
