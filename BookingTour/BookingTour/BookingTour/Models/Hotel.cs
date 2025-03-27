using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class Hotel
{
    public long IdHotel { get; set; }

    public string? Name { get; set; }

    public string? Location { get; set; }

    public string? Description { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Available { get; set; }

    public long? StarRating { get; set; }

    public byte[]? Email { get; set; }

    public string? Image { get; set; }
    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
