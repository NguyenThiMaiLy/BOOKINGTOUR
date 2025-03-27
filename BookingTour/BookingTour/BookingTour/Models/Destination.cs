using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class Destination
{
    public long IdDes { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<TourDetail> TourDetails { get; set; } = new List<TourDetail>();
}
