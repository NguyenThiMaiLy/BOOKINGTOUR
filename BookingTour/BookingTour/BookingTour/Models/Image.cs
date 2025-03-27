using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class Image
{
    public int Id { get; set; }

    public long IdTour { get; set; }

    public string ImageUrl { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}
