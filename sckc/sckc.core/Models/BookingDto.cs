using System.Collections.Generic;

namespace sckc.core.Models
{
    public class BookingDto : BookingRequestDto
    {
        public bool payNow { get; set; }
        public bool isLiveBooking { get; set; }

        public List<BookingItemDto> Items { get; set; }
    }

    public class BookingItemDto
    {
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal Cost { get; set; }
    }
}