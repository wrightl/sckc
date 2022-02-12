using Newtonsoft.Json;
using System;

namespace sckc.api.Models
{
    public class ClubEvent
    {
		[JsonIgnore()]
		public string EventId { get; set; }

		public string Summary { get; set; }

		[JsonIgnore()]
		public string Description { get; set; }

		public DateTime StartDateTime { get; set; }

		public DateTime EndDateTime { get; set; }

		public string Status { get; set; }

		public string LocaleDate { get; set; }

		public string StartDateAsString { get; set; }

		[JsonIgnore()]
		public bool IsSpecialEvent { get; set; }
	}
}
