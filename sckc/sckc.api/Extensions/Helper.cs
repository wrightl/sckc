using sckc.core.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace sckc.api.Extensions
{
    public static class Helper
    {
		public static string MapPath(string path)
		{
			if (path.StartsWith("/"))
			{
				path = path.Substring(1);
			}
			string text = path;
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			path = text.Replace("/", directorySeparatorChar.ToString());
			string physicalApplicationPath = HttpContext.Current.Request.PhysicalApplicationPath;
			return Path.Combine(physicalApplicationPath, path);
		}

		public static async Task<bool> SendMail(SendGridMessage message)
		{
			var apiKey = File.ReadAllText(Helper.MapPath("data/sendgrid_apikey.data"));
			var client = new SendGridClient(apiKey);
			var response = await client.SendEmailAsync(message);

			if (response?.IsSuccessStatusCode == true)
				return true;

			throw new Exception(await response.Body.ReadAsStringAsync());
		}

		public static string ReplaceTemplatePlaceholders(string template, BookingRequestDto info)
        {
			var s = template;

            foreach (var pi in info.GetType().GetProperties())
            {
				s = s.Replace($"{{{{{pi.Name}}}}}", pi.GetValue(info)?.ToString());
            }

			return s;
        }
	}
}