namespace BookingTour.Models
{
    public class PaymentInformationModel
    {
        public string OrderId { get; set; }
        public int Amount { get; set; }
        public string BankCode { get; set; }
    }
}
