using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class Tour
{
    public long IdTour { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Image { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public long? MaxQuantity { get; set; }

    public decimal? Price { get; set; }

    public string? IsDelete { get; set; }

    public long IdType { get; set; }

    public long IdTrans { get; set; }

    public long IdHotel { get; set; }

    public string? ApprovalStatus { get; set; }

    public string? IdUser { get; set; }

    public virtual Hotel? IdHotelNavigation { get; set; }

    public virtual Transportation? IdTransNavigation { get; set; }

    public virtual TypeOfTour? IdTypeNavigation { get; set; }
     

    public virtual ICollection<Image> images { get; set; } = new List<Image>();
    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
    public virtual ICollection<Booking> Booking { get; set; } = new List<Booking>();


    public virtual ICollection<UsersFavoriteTour> UsersFavoriteTours { get; set; } = new List<UsersFavoriteTour>();
    public virtual ICollection<TourDetail> TourDetails { get; set; } = new List<TourDetail>();
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual AspNetUser? IdUserNavigation { get; set; }

}
