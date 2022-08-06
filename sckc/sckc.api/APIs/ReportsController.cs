using Newtonsoft.Json;
using sckc.api.Models;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Http;

namespace sckc.api.APIs
{
    public class ReportsController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get()
        {
            List<Report> reports = new List<Report>();
            string directory = HttpContext.Current.Server.MapPath("/data");
            var filePath = Path.Combine(directory, "tripreports.data");

            if (!Directory.Exists(directory))
            {
                return Ok("Missing folder");
            }
            else if (!File.Exists(filePath))
            {
                return Ok("Missing trip reports");
            }

            reports = JsonConvert.DeserializeObject<List<Report>>(File.ReadAllText(filePath));

            reports.Sort((Report x, Report y) => (!x.date.Equals(y.date) ? y.date.CompareTo(x.date) : 0));

            return Json(reports);
        }

    }
}