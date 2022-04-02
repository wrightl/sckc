﻿using System.Collections.Generic;

namespace sckc.api.Models
{
    public class BookingDto : BookingRequestDto
    {
        public List<BookingItemDto> Items { get; set; }
    }

    public class BookingItemDto
    {
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal Cost { get; set; }
    }
}