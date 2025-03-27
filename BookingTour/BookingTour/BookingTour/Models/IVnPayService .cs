namespace BookingTour.Models
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, Invoice invoices);
        VnPaymentResponseModel PaymentExecute(IQueryCollection collections);
       
    }
}
