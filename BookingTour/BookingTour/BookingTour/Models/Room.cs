using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class Room
{
    public long IdRoom { get; set; }

    public string? RoomNumber { get; set; }

    public string? Status { get; set; }

    public decimal? Price { get; set; }

    public long IdHotel { get; set; }

    public long IdType { get; set; }

    public virtual Hotel IdHotelNavigation { get; set; } = null!;

    public virtual TypeOfRoom IdTypeNavigation { get; set; } = null!;

    public virtual ICollection<RoomDetail> RoomDetails { get; set; } = new List<RoomDetail>();
}
