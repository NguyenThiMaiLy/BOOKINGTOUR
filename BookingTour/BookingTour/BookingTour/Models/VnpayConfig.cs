namespace BookingTour.Models
{
    public class VnpayConfig
    {
        public string TmnCode { get; set; } = "QMRCNKXV";
        public string HashSecret { get; set; } = "LWCH54U91J17J0BUVTME4123M31EZ6EG";
        public string Url { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    }
}
