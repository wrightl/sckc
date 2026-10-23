using System;

namespace sckc.api.Models
{
    // A submitted trip report waiting for someone to accept or reject it from the notification email
    public class PendingReport
    {
        public string id { get; set; }

        // The secret in the accept/reject links; anyone holding it can review this report
        public string token { get; set; }

        public DateTime submitted { get; set; }

        public string submitterEmail { get; set; }

        public Report report { get; set; }
    }
}
