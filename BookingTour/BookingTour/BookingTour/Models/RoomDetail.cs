using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class RoomDetail
{
    public long IdRoom { get; set; }

    public long IdFacility { get; set; }

    public string? Available { get; set; }

    public long? Quanlity { get; set; }

    public string? Condition { get; set; }

    public virtual Facility IdFacilityNavigation { get; set; } = null!;

    public virtual Room IdRoomNavigation { get; set; } = null!;
}
