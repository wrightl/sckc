using System;

namespace sckc.api.Models
{
    public class PhotoSet
    {
        public DateTime created
        {
            get;
            set;
        }

        public string id
        {
            get;
            set;
        }

        public int photoCount
        {
            get;
            set;
        }

        public string primaryPhotoUrl
        {
            get;
            set;
        }

        public string title
        {
            get;
            set;
        }
    }
}