using System.Web.Http;

namespace sckc.api.APIs
{
    public class HealthController : ApiController
	{
		public IHttpActionResult Get()
        {
			return Json("Healthy");
        }
	}
}
