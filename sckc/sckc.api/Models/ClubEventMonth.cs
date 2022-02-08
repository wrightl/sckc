using System.Collections.Generic;
using System.Linq;

namespace sckc.api.Models
{
    public class ClubEventMonth
    {
		public string Month { get; set; }

        public IEnumerable<ClubEvent> Events { get; set; }
	}
}
