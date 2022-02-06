using System;

namespace sckc.api.Models
{
    public class Report
    {
        public DateTime date { get; set; }

        public string id { get; set; }

        public string title { get; set; }

        public string report { get; set; }

        public string trip { get; set; }

        public string coaches { get; set; }

        public string triptype { get; set; }    

        public string organiser { get; set; }

        public string triplength { get; set; }

        public string start { get; set; }

        public string phrase { get; set; }

        public string paddlers { get; set; }    

        public string photos { get; set; }

        public string weather { get; set; }

        public string author { get; set; }

        public bool published { get; set; }
    }
}