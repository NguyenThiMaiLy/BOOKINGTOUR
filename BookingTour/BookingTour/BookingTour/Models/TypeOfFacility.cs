using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class TypeOfFacility
{
    public long IdType { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Facility> Facilities { get; set; } = new List<Facility>();
}
