using System.ComponentModel.DataAnnotations;

namespace sckc.api.Models
{
    // A trip report sent from the website's "Write a trip report" form. The report text is plain text; it is
    // HTML-encoded and split into paragraphs before it is stored, so submissions can never inject markup.
    public class TripReportSubmissionDto
    {
        [Required, StringLength(150)]
        public string title { get; set; }

        // yyyy-MM-dd, as sent by a date input
        [Required, StringLength(10)]
        public string date { get; set; }

        [Required, StringLength(150)]
        public string trip { get; set; }

        [StringLength(100)]
        public string triptype { get; set; }

        [StringLength(100)]
        public string triplength { get; set; }

        [StringLength(150)]
        public string start { get; set; }

        [StringLength(100)]
        public string organiser { get; set; }

        [StringLength(200)]
        public string coaches { get; set; }

        [StringLength(500)]
        public string paddlers { get; set; }

        [StringLength(200)]
        public string weather { get; set; }

        [StringLength(150)]
        public string phrase { get; set; }

        [Required, StringLength(20000, MinimumLength = 50)]
        public string report { get; set; }

        [Required, StringLength(100)]
        public string author { get; set; }

        // So the reviewer can reply with questions; never published
        [Required, EmailAddress, StringLength(200)]
        public string email { get; set; }

        // Hidden from people by the form; anything filled in here came from a spam bot
        public string website { get; set; }
    }
}
