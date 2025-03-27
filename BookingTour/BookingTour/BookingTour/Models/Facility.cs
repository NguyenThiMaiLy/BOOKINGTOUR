    using System;
    using System.Collections.Generic;

namespace BookingTour.Models;

public partial class Facility
{
    public long IdFacility { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public long IdType { get; set; }

    public virtual TypeOfFacility IdTypeNavigation { get; set; } = null!;

    public virtual ICollection<RoomDetail> RoomDetails { get; set; } = new List<RoomDetail>();
}
