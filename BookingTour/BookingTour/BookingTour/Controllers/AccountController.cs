using BookingTour.Data;
using BookingTour.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Mail;
using System.Net;

namespace BookingTour.Controllers
{
    public class AccountController : Controller
    {
        private readonly YourExistingDbContextName _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;
        private readonly UserManager<ApplicationUser> _userManager;
        public AccountController(YourExistingDbContextName context, IHttpContextAccessor httpContextAccessor, IMemoryCache cache, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email)
        {
            // Tìm người dùng dựa trên email (sử dụng UserManager)
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                // Tạo token đặt lại mật khẩu
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Lưu token vào bộ nhớ cache với email người dùng làm khóa
                _cache.Set(email, resetToken, TimeSpan.FromMinutes(30)); // Thời gian hết hạn: 30 phút

                // Gửi email đặt lại mật khẩu với token
                SendResetPasswordEmail(user.Email, resetToken);

                // Chuyển hướng đến trang thông báo gửi email thành công
                return RedirectToAction("ResetPasswordd");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Không tồn tại người dùng với địa chỉ email đã cung cấp!");
                return View();
            }
        }
        private void SendResetPasswordEmail(string userEmail, string resetToken)
        {
            try
            {
                // Thông tin cấu hình SMTP
                string smtpServer = "smtp.gmail.com";
                int smtpPort = 587; // Port mặc định cho SMTP
                string smtpUsername = "tranthebaobt8@gmail.com";
                string smtpPassword = "0378060941T";

                // Tạo đối tượng MailMessage
                using (MailMessage mailMessage = new MailMessage())
                {
                    // Địa chỉ email người gửi
                    mailMessage.From = new MailAddress("your_email@example.com");

                    // Địa chỉ email người nhận
                    mailMessage.To.Add(new MailAddress(userEmail));

                    // Tiêu đề email
                    mailMessage.Subject = "Đặt lại mật khẩu";

                    // Nội dung email
                    mailMessage.Body = $"Nhấp vào liên kết sau để đặt lại mật khẩu của bạn:\n\n" +
                                       $"https://example.com/resetpassword?token={resetToken}";

                    // Gửi email dưới dạng văn bản thuần túy
                    mailMessage.IsBodyHtml = false;

                    // Cấu hình thông tin SMTP và gửi email
                    using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
                    {
                        smtpClient.EnableSsl = true; // Sử dụng SSL để bảo mật kết nối
                        smtpClient.UseDefaultCredentials = false; // Không sử dụng thông tin xác thực mặc định
                        smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword); // Thông tin xác thực SMTP

                        smtpClient.Send(mailMessage); // Gửi email
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý các trường hợp ngoại lệ khi gửi email không thành công
                Console.WriteLine($"Lỗi khi gửi email đặt lại mật khẩu: {ex.Message}");
            }
        }
        [HttpGet]
        public IActionResult ResetPasswordd(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Yêu cầu không hợp lệ.");
                return View();
            }

            var model = new AspNetUser {  Email = email };
            return View(model);
        }
        //[HttpPost]
        //public async Task<IActionResult> ResetPasswordd(AspNetUser model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    // Tìm người dùng dựa trên email
        //    var user = await _userManager.FindByEmailAsync(model.Email);

        //    if (user == null)
        //    {
        //        return RedirectToAction("ResetPasswordConfirmation");
        //    }

        //    // Đặt lại mật khẩu với token và mật khẩu mới
       

        //    if (result.Succeeded)
        //    {
        //        return RedirectToAction("ResetPasswordConfirmation");
        //    }

        //    // Nếu có lỗi, hiển thị chúng
        //    foreach (var error in result.Errors)
        //    {
        //        ModelState.AddModelError("", error.Description);
        //    }

        //    return View(model);
        //}


    }
}
