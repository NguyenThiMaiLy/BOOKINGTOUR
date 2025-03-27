using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class Report
{
    public long IdReport { get; set; }

    public string? Rating { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string Id { get; set; } = null!;

    public long IdTour { get; set; }

    public virtual AspNetUser IdNavigation { get; set; } = null!;


    public virtual Tour IdTourNavigation { get; set; } = null!;
  
}
