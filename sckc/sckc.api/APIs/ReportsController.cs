using Newtonsoft.Json;
using sckc.api.Extensions;
using sckc.api.Models;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace sckc.api.APIs
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        // GET api/<controller>
        [HttpGet]
        public IActionResult Get()
        {
            List<Report> reports = new List<Report>();
            string directory = Helper.MapPath("data");
            var filePath = Path.Combine(directory, "tripreports.data");

            if (!Directory.Exists(directory))
            {
                return Ok("Missing folder");
            }
            else if (!System.IO.File.Exists(filePath))
            {
                return Ok("Missing trip reports");
            }

            reports = JsonConvert.DeserializeObject<List<Report>>(System.IO.File.ReadAllText(filePath));

            reports.Sort((Report x, Report y) => (!x.date.Equals(y.date) ? y.date.CompareTo(x.date) : 0));

            return Ok(reports);
        }

    }
}