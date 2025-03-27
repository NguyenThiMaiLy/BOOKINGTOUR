using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class TypeOfRoom
{
    public long IdType { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
