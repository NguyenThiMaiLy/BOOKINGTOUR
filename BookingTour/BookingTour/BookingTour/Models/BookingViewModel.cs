namespace BookingTour.Models
{
    public class BookingViewModel
    {
        public long IdBooking { get; set; }

        public DateTime? CheckInDate { get; set; }

        public DateTime? CheckOutDate { get; set; }

        public DateTime? BookingTime { get; set; }

        public string Id { get; set; } = null!;

        public long IdHotel { get; set; }

        public long IdTour { get; set; }

        public long IdStatus { get; set; }


        public string? StatusName { get; set; }
        public string CustomerName { get; set; }
        public string TourName { get; set; }
        public DateTime BookingDate { get; set; }

        public string? Description { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
