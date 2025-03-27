using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Twilio.TwiML.Voice;

namespace BookingTour.Models;

public partial class AspNetUser
{
    public string Id { get; set; } = null!;


    public string? UserName { get; set; }

    public string? NormalizedUserName { get; set; }
 
    public string? fullname { get; set; }

    public string? Email { get; set; }

    public string? NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }
  
    [StringLength(512, ErrorMessage = "Địa chỉ không được vượt quá 512 ký tự.")]
    public string? Address { get; set; }
  
    [Range(18, 100, ErrorMessage = "Tuổi phải từ 18 đến 100.")]
    public string? Age { get; set; }

    public string? AvatarUrl { get; set; }
    //public string? Fullname { get; set; }

    public virtual ICollection<Tour> Tours { get; set; }
    public virtual ICollection<Promotion> IdPmts { get; set; } = new List<Promotion>();
    public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();
    public virtual ICollection<UsersFavoriteTour> UsersFavoriteTours { get; set; } = new List<UsersFavoriteTour>();
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<AspNetUserToken> AspNetUserTokens { get; set; } = new List<AspNetUserToken>();
    public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; } = new List<AspNetUserLogin>();

    public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; } = new List<AspNetUserClaim>();

}
