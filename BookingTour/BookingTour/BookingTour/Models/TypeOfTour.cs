using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class TypeOfTour
{
    public long IdType { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
