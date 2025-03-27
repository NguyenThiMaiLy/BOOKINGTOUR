using BookingTour.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using MimeKit;
using BookingTour.Models;

namespace BookingTour.Areas.Host.Controllers
{
    [Route("Host/Report")]
    [Area("Host")]
    public class ReportController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly YourExistingDbContextName _context;
        private readonly IEmailSender _emailSender;
        public ReportController(YourExistingDbContextName context, IEmailSender emailSender, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager;
        }
        public async Task<IActionResult> TourReports(int pageNumber = 1, int pageSize = 10, string searchTerm = "")
        {
            // Lấy Id của người dùng hiện tại từ User.Identity.Name
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Người dùng chưa đăng nhập!";
                return RedirectToAction("Login", "Account");
            }

            // Bước 1: Lấy danh sách các tour mà người dùng sở hữu
            var userTours = await _context.Tours
                .Where(t => t.IdUser == userId)
                .Select(t => t.IdTour)
                .ToListAsync();

            // Bước 2: Lọc các báo cáo theo IdTour từ bảng Report
            // Truy vấn ban đầu với Include
            var query = _context.Reports
                .Where(r => userTours.Contains(r.IdTour)) // Lọc theo IdTour
                .Include(r => r.IdTourNavigation)        // Bao gồm thông tin tour
                .Include(r => r.IdNavigation)            // Bao gồm thông tin người dùng (AspNetUser)
                .AsQueryable();                          // Chuyển đổi thành IQueryable để thêm điều kiện sau

            // Áp dụng điều kiện tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r =>
                    r.IdTourNavigation.Name.Contains(searchTerm) || // Tên tour
                    r.IdNavigation.fullname.Contains(searchTerm));  // Tên người dùng
            }

            // Bước 3: Phân trang
            var totalReports = await query.CountAsync();
            var reports = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Bước 4: Truyền dữ liệu vào View
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalReports / pageSize);
            ViewBag.SearchTerm = searchTerm;

            return View(reports);
        }
        [HttpPost("RespondToReport")]
        public async Task<IActionResult> RespondToReport(long reportId, string responseMessage)
        {
            if (string.IsNullOrEmpty(responseMessage))
            {
                // Thông báo lỗi nếu phản hồi trống
                TempData["Error"] = "Vui lòng nhập phản hồi!";
                return RedirectToAction("TourReports"); // Quay lại trang danh sách báo cáo
            }

            // Lấy thông tin báo cáo từ cơ sở dữ liệu
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
            {
                return NotFound();
            }

            // Lấy người dùng đã gửi báo cáo
            var user = await _userManager.FindByIdAsync(report.Id);
            if (user == null)
            {
                return NotFound();
            }

            // Tạo email phản hồi
            var emailMessage = new MailMessage
            {
                From = new MailAddress("tranthebaobt8@gmail.com", "AdminBaotran"),
                Subject = "Phản hồi báo cáo của bạn",
                Body = $"Chào {user.UserName},<br><br>" +
                       $"Cảm ơn bạn đã gửi báo cáo. Đây là phản hồi của chúng tôi: <br><br>" +
                       $"<strong>Phản hồi:</strong><br>{responseMessage}<br><br>" +
                       "Trân trọng,<br>Admin",
                IsBodyHtml = true, // Cho phép gửi email dưới dạng HTML
            };

            emailMessage.To.Add(new MailAddress(user.Email)); // Thêm email người nhận

            // Gửi email qua SMTP
            using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
            {
                try
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.Credentials = new NetworkCredential("tranthebaobt8@gmail.com", "kaiv zped dlrx xfgx"); // Mật khẩu ứng dụng
                    await smtpClient.SendMailAsync(emailMessage);

                    // Hiển thị thông báo thành công
                    TempData["Success"] = "Phản hồi đã được gửi thành công!";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Có lỗi khi gửi email: " + ex.Message;
                }
            }

            // Quay lại trang danh sách báo cáo
            return RedirectToAction("TourReports");
        }



    }
}
