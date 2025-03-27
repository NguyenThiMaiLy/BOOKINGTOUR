using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class Promotion
{
    public long IdPmt { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal? DiscountAmount { get; set; }

    public string? EligibilityCriteria { get; set; }

    public string? Status { get; set; }

    public long IdTour { get; set; }

    public long IdHotel { get; set; }
    public virtual Hotel IdHotelNavigation { get; set; } 
    public virtual Tour IdTourNavigation { get; set; } 
    public virtual ICollection<AspNetUser> Ids { get; set; } = new List<AspNetUser>();


}
