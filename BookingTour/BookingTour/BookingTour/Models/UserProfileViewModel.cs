namespace BookingTour.Models
{
    public class UserProfileViewModel
    {

        public string Id { get; set; } = null!;
        public string? UserName { get; set; }
        public string? fullname { get; set; }
        public string? Address { get; set; }
        public string? Age { get; set; }
        public string? AvatarUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? NormalizedEmail { get; set; }

        public bool EmailConfirmed { get; set; }

        public string? PasswordHash { get; set; }

        public string? SecurityStamp { get; set; }

        public string? ConcurrencyStamp { get; set; }



        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public DateTimeOffset? LockoutEnd { get; set; }

        public bool LockoutEnabled { get; set; }

        public int AccessFailedCount { get; set; }
        public virtual Hotel? IdHotelNavigation { get; set; }

        public virtual Transportation? IdTransNavigation { get; set; }

        public virtual TypeOfTour? IdTypeNavigation { get; set; }

        public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; } = new List<AspNetUserClaim>();

        public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; } = new List<AspNetUserLogin>();

        public virtual ICollection<AspNetUserToken> AspNetUserTokens { get; set; } = new List<AspNetUserToken>();

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

        public virtual ICollection<UsersFavoriteTour> UsersFavoriteTours { get; set; } = new List<UsersFavoriteTour>();

        public virtual ICollection<Promotion> IdPmts { get; set; } = new List<Promotion>();

        public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();
        public List<Promotion> Promotions { get; set; }
        public List<Tour> FavoriteTours { get; set; }
    }
}
