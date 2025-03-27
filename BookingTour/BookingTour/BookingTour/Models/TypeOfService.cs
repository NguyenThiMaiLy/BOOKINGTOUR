using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class TypeOfService
{
    public long IdType { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
