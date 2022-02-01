using Newtonsoft.Json;
using sckc.api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace sckc.api.APIs
{
    public class GalleryController : ApiController
    {
        [HttpGet("GetName")]
        public IHttpActionResult GetName()
        {
            return Ok("Worked");
        }

        // GET api/<controller>
        public IHttpActionResult Get()
        {
            string str;
            List<PhotoSet> photoSets = new List<PhotoSet>();
            string str1 = HttpContext.Current.Server.MapPath("/gallery");

            if (!Directory.Exists(str1))
            {
                return Ok("Missing folder");
            }

            string[] directories = Directory.GetDirectories(str1);
            for (int i = 0; i < (int)directories.Length; i++)
            {
                string str2 = directories[i];
                string fileName = Path.GetFileName(str2);
                string[] strArrays = fileName.Split("@".ToCharArray());
                if ((int)strArrays.Length == 2)
                {
                    string[] files = Directory.GetFiles(str2);
                    if (files.Length != 0)
                    {
                        if (!File.Exists(Path.Combine(str2, "primary.jpg")))
                        {
                            str = (!File.Exists(Path.Combine(str2, "primary.jpeg")) ? Path.GetFileName(files[0]) : "primary.jpeg");
                        }
                        else
                        {
                            str = "primary.jpg";
                        }
                        DateTime dateTime = DateTime.Parse(strArrays[0]);
                        photoSets.Add(new PhotoSet()
                        {
                            id = fileName,
                            created = dateTime,
                            photoCount = (int)files.Length,
                            primaryPhotoUrl = string.Concat("gallery/", fileName, "/", str),
                            title = strArrays[1].Replace("_", " ")
                        });
                    }
                }
            }
            photoSets.Sort((PhotoSet x, PhotoSet y) => (!x.created.Equals(y.created) ? y.created.CompareTo(x.created) : 0));
            return Json(photoSets);
        }

    }
}