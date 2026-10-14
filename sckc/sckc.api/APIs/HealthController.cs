using Microsoft.AspNetCore.Mvc;

namespace sckc.api.APIs
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
	{
		[HttpGet]
		public IActionResult Get()
        {
			return Ok("Healthy");
        }
	}
}
