using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class Service
{
    public long IdService { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? Available { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long IdTour { get; set; }

    public long IdHotel { get; set; }

    public long IdType { get; set; }

    public virtual Hotel IdHotelNavigation { get; set; } = null!;

    public virtual Tour IdTourNavigation { get; set; } = null!;

    public virtual TypeOfService IdTypeNavigation { get; set; } = null!;
}
