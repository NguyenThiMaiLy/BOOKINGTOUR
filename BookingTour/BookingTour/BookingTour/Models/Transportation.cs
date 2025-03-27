using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class Transportation
{
    public long IdTrans { get; set; }

    public string? Name { get; set; }

    public string? Type { get; set; }

    public long? SeatAvailability { get; set; }

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
