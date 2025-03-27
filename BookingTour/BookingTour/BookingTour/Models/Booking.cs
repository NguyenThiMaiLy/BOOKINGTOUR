using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class Booking
{
    public long IdBooking { get; set; }

    public DateTime? CheckInDate { get; set; }

    public DateTime? CheckOutDate { get; set; }

    public DateTime? BookingTime { get; set; }

    // Khóa ngoại tới bảng người dùng
    public string Id { get; set; } = null!;

    // Khóa ngoại tới bảng Hotel
    public long IdHotel { get; set; }

    // Khóa ngoại tới bảng Tour
    public long IdTour { get; set; } 

    // Khóa ngoại tới bảng BookingStatus
    public long IdStatus { get; set; }

    // Điều hướng tới bảng Hotel
    public virtual Hotel IdHotelNavigation { get; set; } 

    // Điều hướng tới bảng AspNetUser (người dùng)
    public virtual AspNetUser IdNavigation { get; set; } 

    // Điều hướng tới bảng BookingStatus
    public virtual BookingStatus IdStatusNavigation { get; set; } 

    // Điều hướng tới bảng Tour
    public virtual Tour IdTourNavigation { get; set; }
    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
    public long? IdInvoice { get; set; }  // Cho phép null nếu không có hóa đơn

    public virtual Invoice? InvoiceNavigation { get; set; }
}   