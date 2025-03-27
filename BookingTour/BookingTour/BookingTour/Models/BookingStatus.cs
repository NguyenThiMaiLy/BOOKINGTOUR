using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class BookingStatus
{
    public long IdStatus { get; set; }

    public string? StatusName { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
