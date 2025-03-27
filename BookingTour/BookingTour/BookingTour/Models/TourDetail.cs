using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class TourDetail
{
    public long IdTour { get; set; }

    public long IdDes { get; set; }

    public string? Itinerary { get; set; }

    public virtual Destination IdDesNavigation { get; set; }    

    public virtual Tour IdTourNavigation { get; set; } = null!;
}
