using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Newtonsoft.Json.Linq;
using sckc.api.Extensions;
using sckc.api.Models;

namespace sckc.api.APIs
{
	public class EventsController : ApiController
	{
		[Route("api/GetEvents")]
		public IHttpActionResult Get(int count = 10)
		{
			return Json(CalendarServiceHelper.GetClubEvents(count));
		}

		[Route("api/GetGroupedEvents")]
		public IHttpActionResult GetGroupedEvent()
		{
			return Json(CalendarServiceHelper.GetGroupedClubEvents());
		}
	}

	public static class CalendarServiceHelper
	{

		private static string BookingsCalendarId = "6ao4i5trvae0te3v63csl39208@group.calendar.google.com";
		private static string ClubCalendarId = "sheffieldcitykayakclub@gmail.com";
		private static string SpecialEventsCalendarId = "n6fiiet0qjudl4obhnldfj6r4@group.calendar.google.com";

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

		public static Event SaveToGoogleCalendar(JObject Booking)
		{
			try
			{
				CalendarService calendarService = CreateGoogleCalendarService();
				string description = string.Format("Booking request received from {0} for {1} ({2} adult/s, {3} children)", Booking["email"].Value<string>(), Booking["names"].Value<string>(), Booking["adults"].Value<string>(), Booking["children"].Value<string>());
				EventsResource.InsertRequest insertRequest = calendarService.Events.Insert(new Event
				{
					Description = description,
					Start = new EventDateTime
					{
						Date = Booking["date"].Value<string>()
					},
					End = new EventDateTime
					{
						Date = Booking["date"].Value<string>()
					},
					Summary = Booking["names"].Value<string>() + " (" + Booking["adults"].Value<string>() + " adult/s, " + Booking["children"].Value<string>() + " children)",
					ColorId = "5",
					Status = "tentative",
					Attendees = new List<EventAttendee>
					{
						new EventAttendee
						{
							Email = Booking["email"].Value<string>()
						}
					}
				}, BookingsCalendarId);
				return insertRequest.Execute();
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static IEnumerable<ClubEventMonth> GetGroupedClubEvents()
        {
			return GetClubEvents().GroupBy(ev => ev.StartDateTime.ToString("MMMMYYYY"))
							 .Select((key, group) => new ClubEventMonth()
							 { Events = key.OrderBy(ev => ev.StartDateTime).ToList(), Month = key.FirstOrDefault()?.StartDateTime.ToString("MMMM") });

		}

		public static IEnumerable<ClubEvent> GetClubEvents(int? count = null)
		{
			CalendarService calendarService = CreateGoogleCalendarService();
			EventsResource.ListRequest listRequest = calendarService.Events.List(ClubCalendarId);
			listRequest.TimeMin = DateTime.Today;
			listRequest.TimeMax = DateTime.Today.AddMonths(6);
			Events events2 = listRequest.Execute();
			List<ClubEvent> list = new List<ClubEvent>();
			foreach (Event item in events2.Items)
			{
				ProcessEvent(item, list);
			}
			//try
			//{
			//	listRequest = calendarService.Events.List(SpecialEventsCalendarId);
			//	listRequest.TimeMin = DateTime.Today;
			//	events2 = listRequest.Execute();
			//	List<ClubEvent> list2 = new List<ClubEvent>();
			//	foreach (Event item2 in events2.Items)
			//	{
			//		ProcessEvent(item2, list2);
			//	}
			//	list2.ForEach(delegate (ClubEvent entry)
			//	{
			//		entry.IsSpecialEvent = true;
			//	});
			//	list.AddRange(list2);
			//}
			//catch (Exception ex)
			//{

			//}

			list = list.Where(ev => !ev.Status.Equals("cancelled")).ToList();

			if (count != null && count.HasValue)
				list = list.Take(count.Value).ToList();

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

		private static void ProcessEvent(Event evt, List<ClubEvent> events)
		{
			if (!string.IsNullOrWhiteSpace(evt.RecurringEventId))
			{
				DateTime originalStartAt = GetDateTime(evt.OriginalStartTime);
				ClubEvent clubEvent = events.FirstOrDefault((ClubEvent entry) => entry.EventId.Equals(evt.RecurringEventId) && originalStartAt.Date.Equals(entry.StartDateTime.Date));
				if (clubEvent != null)
				{
					events.Remove(clubEvent);
				}
			}
			DateTime dateTime = GetDateTime(evt.Start);
			if (dateTime == DateTime.MinValue)
			{
				return;
			}
			DateTime dateTime2 = GetDateTime(evt.End);
			if (evt.Recurrence != null && evt.Recurrence.Count > 0)
			{
				foreach (string item in evt.Recurrence)
				{
					if (item.Contains(":") && item.Length > 2)
					{
						string[] array = item.Split(":".ToCharArray());
						string[] recurrenceParamsArray = array[1].Split(";".ToCharArray());
						string text = array[0];
						if (!(text == "RRULE"))
						{
							if (text == "...")
							{
							}
						}
						else
						{
							ParseRecurrenceEntry(recurrenceParamsArray, out var count, out var freq, out var interval, out var until, out var byday);
							if (until == DateTime.MinValue)
							{
								until = DateTime.Today.AddMonths(6);
							}
							if (until > DateTime.Today)
							{
								switch (freq)
								{
									case "DAILY":
										{
											DateTime dateTime3 = dateTime;
											if (interval <= 0)
											{
												interval = 1;
											}
											int num = 0;
											while (dateTime3 <= until)
											{
												if (dateTime3 >= DateTime.Today && AddEvent(evt, dateTime3, new DateTime(dateTime3.Year, dateTime3.Month, dateTime3.Day, dateTime2.Hour, dateTime2.Minute, 0), events))
												{
													num++;
												}
												dateTime3 = dateTime3.AddDays(interval);
												if (count > 0 && num >= count)
												{
													break;
												}
											}
											break;
										}
									case "WEEKLY":
										{
											DateTime dateTime3 = dateTime;
											if (interval <= 0)
											{
												interval = 1;
											}
											int num = 0;
											while (dateTime3 <= until)
											{
												if (dateTime3 >= DateTime.Today && CheckDay(dateTime3, byday) && AddEvent(evt, dateTime3, new DateTime(dateTime3.Year, dateTime3.Month, dateTime3.Day, dateTime2.Hour, dateTime2.Minute, 0), events))
												{
													num++;
												}
												dateTime3 = dateTime3.AddDays(interval);
												if (count > 0 && num >= count)
												{
													break;
												}
											}
											break;
										}
									case "MONTHLY":
										{
											DateTime dateTime3 = dateTime;
											if (interval <= 0)
											{
												interval = 1;
											}
											int num = 0;
											while (dateTime3 <= until)
											{
												if (dateTime3 >= DateTime.Today && CheckDay(dateTime3, byday) && AddEvent(evt, dateTime3, new DateTime(dateTime3.Year, dateTime3.Month, dateTime3.Day, dateTime2.Hour, dateTime2.Minute, 0), events))
												{
													num++;
												}
												dateTime3 = dateTime3.AddMonths(interval);
												if (count > 0 && num >= count)
												{
													break;
												}
											}
											break;
										}
									case "YEARLY":
										{
											DateTime dateTime3 = dateTime;
											if (interval <= 0)
											{
												interval = 1;
											}
											int num = 0;
											while (dateTime3 <= until)
											{
												if (dateTime3 >= DateTime.Today && CheckDay(dateTime3, byday) && AddEvent(evt, dateTime3, new DateTime(dateTime3.Year, dateTime3.Month, dateTime3.Day, dateTime2.Hour, dateTime2.Minute, 0), events))
												{
													num++;
												}
												dateTime3 = dateTime3.AddYears(interval);
												if (count > 0 && num >= count)
												{
													break;
												}
											}
											break;
										}
								}
							}
						}
					}
				}
				return;
			}
			AddEvent(evt, dateTime, dateTime2, events);
		}

		private static bool CheckDay(DateTime dt, string byday)
		{
			if (string.IsNullOrWhiteSpace(byday))
			{
				return false;
			}
			switch (byday.ToUpper())
			{
				case "MO":
					return dt.DayOfWeek == DayOfWeek.Monday;
				case "TU":
					return dt.DayOfWeek == DayOfWeek.Tuesday;
				case "WE":
					return dt.DayOfWeek == DayOfWeek.Wednesday;
				case "TH":
					return dt.DayOfWeek == DayOfWeek.Thursday;
				case "FR":
					return dt.DayOfWeek == DayOfWeek.Friday;
				case "SA":
					return dt.DayOfWeek == DayOfWeek.Saturday;
				case "SU":
					return dt.DayOfWeek == DayOfWeek.Sunday;
				default:
					{
						if (int.TryParse(byday, out var result))
						{
							return dt.Day == result;
						}
						return false;
					}
			}
		}

		private static void ParseRecurrenceEntry(string[] recurrenceParamsArray, out int count, out string freq, out int interval, out DateTime until, out string byday)
		{
			count = int.MinValue;
			freq = string.Empty;
			interval = int.MinValue;
			until = DateTime.MinValue;
			byday = string.Empty;
			foreach (string text in recurrenceParamsArray)
			{
				if (text.Contains("=") && text.Length > 2)
				{
					string[] array = text.Split("=".ToCharArray());
					switch (array[0].ToUpper())
					{
						case "COUNT":
							count = int.Parse(array[1]);
							break;
						case "FREQ":
							freq = array[1].ToUpper();
							break;
						case "INTERVAL":
							interval = int.Parse(array[1]);
							break;
						case "BYDAY":
							byday = array[1];
							break;
						case "UNTIL":
							until = ConvertUntil(array[1]);
							break;
					}
				}
			}
		}

		private static DateTime ConvertUntil(string until)
		{
			return new DateTime(int.Parse(until.Substring(0, 4)), int.Parse(until.Substring(4, 2)), int.Parse(until.Substring(6, 2)));
		}

		private static bool AddEvent(Event evt, DateTime StartAt, DateTime EndAt, List<ClubEvent> events)
		{
			try
			{
				if (StartAt >= DateTime.Today)
				{
					ClubEvent item = new ClubEvent
					{
						EventId = evt.Id,
						Summary = evt.Summary,
						Description = evt.Description,
						StartDateTime = StartAt,
						EndDateTime = EndAt,
						Status = evt.Status,
						LocaleDate = StartAt.ToString("dd/MM/yy"),
						StartDateAsString = StartAt.ToString("yyyy-MM-dd HH:mm")
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

		public static string GetBaseUrl()
		{
			string text = HttpContext.Current.Request.ApplicationPath;
			if (!string.IsNullOrWhiteSpace(text) && text.StartsWith("/"))
			{
				text = text.Substring(1);
			}
			if (!string.IsNullOrWhiteSpace(text) && text.EndsWith("/"))
			{
				text = text.Substring(0, text.Length - 1);
			}
			string leftPart = HttpContext.Current.Request.Url
				.GetLeftPart(UriPartial.Authority);
			return leftPart + "/" + text;
		}
	}
}
