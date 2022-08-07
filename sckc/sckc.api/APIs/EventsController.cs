using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using sckc.api.Extensions;
using sckc.api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;

namespace sckc.api.APIs
{
    public class EventsController : ApiController
	{
        [Route("api/GetEvents")]
        public IHttpActionResult GetEvents(int count = 10)
		{
			count = validateCount(count);

			return Json(CalendarServiceHelper.GetClubEvents(count));
		}

		[Route("api/GetEventsOfType")]
        public IHttpActionResult GetEventsOfType(string type, int count = 5)
		{
			count = validateCount(count);

			var events = CalendarServiceHelper.GetClubEvents().Where(ev => ev.EventType.ToLowerInvariant().Contains(type)).Take(count);

			return Json(events);
		}

        [Route("api/GetGroupedEvents")]
        public IHttpActionResult GetGroupedEvent()
		{
			return Json(CalendarServiceHelper.GetGroupedClubEvents());
		}

		private int validateCount(int count)
        {
			if (count <= 5)
				count = 5;
			else if (count > 100)
				count = 100;

			return count;
		}
	}

	public static class CalendarServiceHelper
	{

		private static string ClubCalendarId = Constants.GoogleCalendarId;

		public static CalendarService CreateGoogleCalendarService()
		{
			GoogleCredential googleCredential;
			using (Stream stream = new FileStream(Helper.MapPath("data/sckc.data"), FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				googleCredential = GoogleCredential.FromStream(stream);
			}
			string[] scopes = new string[1] { CalendarService.Scope.CalendarReadonly };
			googleCredential = googleCredential.CreateScoped(scopes);
			return new CalendarService(new BaseClientService.Initializer
			{
				HttpClientInitializer = googleCredential,
				ApplicationName = "sckc"
			});
		}

		public static IEnumerable<ClubEventMonth> GetGroupedClubEvents()
        {
			return GetClubEvents().GroupBy(ev => ev.StartDateTime.ToString("MMMMyyyy"))
							 .Select((key, group) => new ClubEventMonth()
							 { Events = key.OrderBy(ev => ev.StartDateTime).ToList(), Month = key.FirstOrDefault()?.StartDateTime.ToString("MMMM") });
		}

		public static IEnumerable<ClubEvent> GetClubEvents(int? count = null)
		{
			CalendarService calendarService = CreateGoogleCalendarService();
			var request = calendarService.Events.List(ClubCalendarId);
			request.TimeMin = DateTime.Now;
			request.TimeMax = DateTime.Today.AddMonths(12);
			request.SingleEvents = true;
			request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
			
			var results = request.Execute();
			List<ClubEvent> list = new List<ClubEvent>();
			foreach (Event item in results.Items)
			{
                AddEvent(item, list);
            }

			if (count != null && count.HasValue && list.Count > count)
				return list.Take(count.Value);

			return list;
		}

		private static DateTime GetDateTime(EventDateTime dt)
		{
			if (dt == null)
			{
				return DateTime.MinValue;
			}
			if (dt.DateTime.HasValue && dt.DateTime.HasValue)
			{
				return dt.DateTime.Value;
			}
			return DateTime.Parse(dt.Date);
		}

		private static bool AddEvent(Event evt, List<ClubEvent> events)
		{
			try
			{
				var startAt = GetDateTime(evt.Start);
				var endAt = GetDateTime(evt.End);

				if (startAt >= DateTime.Today)
				{
					var eventType = evt.Summary.ToLowerInvariant();
                    if(eventType.Contains("pool"))
                    {
						eventType = "pool";
                    }
					else if(eventType.Contains("river"))
                    {
						eventType = "river";
                    }
                    else
                    {
						eventType = "event";
                    }

                    ClubEvent item = new ClubEvent
					{
						EventId = evt.Id,
						Summary = evt.Summary,
						Description = evt.Description,
						StartDateTime = startAt,
						EndDateTime = endAt,
						Status = evt.Status,
						LocaleDate = startAt.ToString("dd/MM/yy"),
						StartDateAsString = startAt.ToString("yyyy-MM-dd HH:mm"),
						EventType = eventType
					};
					events.Add(item);
				}
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
