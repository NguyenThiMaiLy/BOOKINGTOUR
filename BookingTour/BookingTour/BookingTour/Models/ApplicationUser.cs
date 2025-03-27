using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BookingTour.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]

        public string? fullname { get; set; }
        public string? Address { get; set; }
        public string? Age { get; set; }
        public string? AvatarUrl { get; set; }
   
        //public string? Otp { get; set; }
        //public DateTime OtpExpiryTime { get; set; }

    }
}
