namespace BookingTour.Models
{
    public class PaymentConfirmationViewModel
    {
        public long IdInvoice { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime BillingDate { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRefunded { get; set; }
    }
}
