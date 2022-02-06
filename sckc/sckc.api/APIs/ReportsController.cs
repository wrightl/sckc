using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using sckc.api.Models;

namespace sckc.api.APIs
{
    public class ReportsController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get()
        {
            List<Report> reports = new List<Report>();
            string directory = HttpContext.Current.Server.MapPath("/data");

            if (!Directory.Exists(directory))
            {
                return Ok("Missing folder");
            }

            reports = JsonConvert.DeserializeObject<List<Report>>(File.ReadAllText(Path.Combine(directory, "tripreports.data")));

            reports.Sort((Report x, Report y) => (!x.date.Equals(y.date) ? y.date.CompareTo(x.date) : 0));

            return Json(reports);
        }

    }
}