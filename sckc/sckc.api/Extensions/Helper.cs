using sckc.api.Models;
using sckc.core.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace sckc.api.Extensions
{
    public static class Helper
    {
		public static string ContentRootPath { get; set; } = AppContext.BaseDirectory;

		public static string MapPath(string path)
		{
			if (path.StartsWith("/"))
			{
				path = path.Substring(1);
			}
			string text = path;
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			path = text.Replace("/", directorySeparatorChar.ToString());
			return Path.Combine(ContentRootPath, path);
		}

		public const string ResendApiKeyFile = "data/resend_apikey.data";
		private const string ResendSendUrl = "https://api.resend.com/emails";

		// Replaced by the tests so they can check what would be sent without calling Resend
		public static Func<EmailMessage, Task<bool>> MailSender { get; set; } = SendWithResend;

		// One client for the app's lifetime; the tests swap it for one with a fake handler
		public static HttpClient ResendClient { get; set; } = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

		public static Task<bool> SendMail(EmailMessage message)
		{
			return MailSender(message);
		}

		// Sends through Resend's API (https://resend.com/docs/api-reference/emails/send-email). Throws with Resend's
		// own error message on failure, which the global error handler logs and reports.
		public static async Task<bool> SendWithResend(EmailMessage message)
		{
			var keyPath = MapPath(ResendApiKeyFile);
			if (!File.Exists(keyPath))
			{
				throw new InvalidOperationException($"Email can't be sent: {ResendApiKeyFile} is missing");
			}

			var request = new HttpRequestMessage(HttpMethod.Post, ResendSendUrl)
			{
				Content = JsonContent.Create(new
				{
					from = message.From.ToString(),
					to = message.To.Select(a => a.ToString()).ToArray(),
					subject = message.Subject,
					html = message.Html,
					text = message.Text,
					reply_to = message.ReplyTo?.ToString(),
				}, options: new System.Text.Json.JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }),
			};
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", File.ReadAllText(keyPath).Trim());

			using var response = await ResendClient.SendAsync(request);
			if (response.IsSuccessStatusCode)
			{
				return true;
			}

			throw new Exception($"Resend refused the email ({(int)response.StatusCode}): {await response.Content.ReadAsStringAsync()}");
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
