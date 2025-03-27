using BookingTour.Data;
using BookingTour.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookingTour.Controllers
{
    public class PaymentController : Controller
    {

        private readonly VnpayService _vnpayService;
        private readonly YourExistingDbContextName _context;
        public PaymentController(VnpayService vnpayService, YourExistingDbContextName context)
        {
            _vnpayService = vnpayService;
            _context = context;
        }
        public IActionResult PaymentDetail(long invoiceId)
        {
            var invoice = _context.Invoices
                .Include(i => i.PaymentInvoices)  // Bao gồm các PaymentInvoice liên quan đến Invoice
                .FirstOrDefault(i => i.IdInvoice == invoiceId);

            if (invoice == null)
            {
                return NotFound();
            }

            // Trả về view và gửi dữ liệu Invoice cùng với danh sách PaymentInvoice
            return View(invoice);
        }

        [HttpPost]
        public IActionResult payment(decimal total,long id)
        {
            var invoice = new Invoice
            {
                IdInvoice = id,
                Description = "thanh toan",
                TotalAmount = total,
                BillingDate = DateTime.Now,
                CreatedAt = DateTime.Now,

            };
           
            return Redirect(_vnpayService.CreatePaymentUrl(HttpContext, invoice));
        }
        public IActionResult PaymentCallback()
        {
            // Gọi phương thức PaymentExecute để xử lý phản hồi từ VnPay
            var response = _vnpayService.PaymentExecute(Request.Query);

            // Kiểm tra nếu phản hồi không thành công hoặc mã phản hồi không phải "00"
            if (response == null || response.VnPayResponseCode != "00")
            {
                return RedirectToAction(nameof(Failure));
            }
            var invoice = _context.Invoices.FirstOrDefault(i => i.IdInvoice == response.OrderDescription);
            if (invoice == null)
            {
                // Nếu không tìm thấy hóa đơn, chuyển đến trang thất bại
                return RedirectToAction(nameof(Failure));
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Tạo đối tượng Payment để lưu thông tin thanh toán vào cơ sở dữ liệu
            var payment = new Payment
            {
                IdPayment = _context.Payments.Max(p => p.IdPayment) + 1,
                Date = DateTime.Now,
                Method = response.PaymentMethod,
                Amount = invoice.TotalAmount,
                Status = "Completed", // Trạng thái thanh toán thành công
                IsRefunded = "N",
                Id = userId // ID của người dùng hoặc hóa đơn (tuỳ thuộc vào cách bạn lưu trữ)
            };

            // Lưu Payment vào cơ sở dữ liệu
            _context.Payments.Add(payment);
            _context.SaveChanges();

            // Tạo đối tượng PaymentInvoice nếu cần lưu thêm thông tin vào bảng PaymentInvoice
            var paymentInvoice = new PaymentInvoice
            {
                IdPayment = payment.IdPayment, // ID của thanh toán vừa được lưu
                IdInvoice = invoice.IdInvoice, // ID của hóa đơn
                PaidAmount = invoice.TotalAmount,
                PaymentDate = DateTime.Now,
                Status = "Completed"
            };

            // Lưu PaymentInvoice vào cơ sở dữ liệu
            _context.PaymentInvoices.Add(paymentInvoice);
            _context.SaveChanges();
            TempData["PopupMessage"] = "Bạn đã Thanh toán thành công thành công!";
            // Chuyển hướng đến trang thành công
            return RedirectToAction("Index","Tours");
        }


        //[HttpGet]
        //public async Task<IActionResult> ReturnFromVnpay()
        //{
        //    var vnp_Params = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
        //    string vnp_ResponseCode = vnp_Params["vnp_ResponseCode"];
        //    string vnp_TxnRef = vnp_Params["vnp_TxnRef"];
          
        //    // Kiểm tra mã phản hồi và xử lý lưu dữ liệu thanh toán vào cơ sở dữ liệu
        //    if (vnp_ResponseCode == "00")
        //    {
        //        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //        // Thanh toán thành công
        //        var payment = new Payment
        //        {
        //            Date = DateTime.Now,
        //            Amount = decimal.Parse(vnp_Params["vnp_Amount"]) / 100,
        //            Status = "Success",
        //            Method = "VNPAY",
        //            Id = userId,
        //            IsRefunded = "N",// Lấy id người dùng từ session hoặc thông tin đăng nhập
        //        };
        //        payment.IdPayment = await _context.Payments.Select(p => p.IdPayment).DefaultIfEmpty(0).MaxAsync() + 1;
        //        // Lưu dữ liệu vào cơ sở dữ liệu
        //        _context.Payments.Add(payment);
        //        await _context.SaveChangesAsync();
        //        var paymentInvoice = new PaymentInvoice
        //        {
        //            IdPayment = payment.IdPayment,
        //            IdInvoice = 12345, // Tham chiếu đến ID hóa đơn (cần xác định từ nơi khác)
        //            PaidAmount = payment.Amount,
        //            PaymentDate = payment.Date,
        //            Status = "Paid"
        //        };

        //        _context.PaymentInvoices.Add(paymentInvoice);
        //        await _context.SaveChangesAsync();

        //        // Xử lý thêm sau khi lưu thành công, ví dụ: redirect tới trang xác nhận
        //        return RedirectToAction("Success", "Payment");
        //    }
        //    else
        //    {
        //        return RedirectToAction("Failure", "Payment");
        //    }

       
        //}
        public IActionResult Success()
        {
            return View();
        }

        // Trang thất bại nếu thanh toán không thành công
        public IActionResult Failure()
        {
            return View();
        }
    }
}