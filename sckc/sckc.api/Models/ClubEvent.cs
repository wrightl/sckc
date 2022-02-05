using System;

namespace sckc.api.Models
{
    public class ClubEvent
    {
		public string EventId { get; set; }

		public string Summary { get; set; }

		public string Description { get; set; }

		public DateTime StartDateTime { get; set; }

		public DateTime EndDateTime { get; set; }

		public string Status { get; set; }

		public string LocaleDate { get; set; }

		public string StartDateAsString { get; set; }

		public bool IsSpecialEvent { get; set; }
	}
}
