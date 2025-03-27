using System;
using System.Collections.Generic;

namespace BookingTour.Models
{
    public class HostRegistration
    {
        public int Id { get; set; }

     
        public string UserId { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; }
        public bool IsApproved { get; set; } // Trạng thái phê duyệt

        // Điều này giúp Entity Framework biết rằng UserId là khóa ngoại
        public virtual AspNetUser User { get; set; }
    }
}
