using BookingTour.Data;
using BookingTour.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity;

namespace BookingTour.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = CD.Role_Admin)]
    public class HostManagementController : Controller
    {
        private readonly YourExistingDbContextName _context;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        public HostManagementController(YourExistingDbContextName context, IHttpContextAccessor httpContextAccessor, IMemoryCache cache, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> PendingRegistrations(int page = 1, int pageSize = 10, string searchTerm = "")
        {
            // Tính tổng số bản ghi sau khi tìm kiếm
            var query = _context.HostRegistrations.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(h => h.FullName.Contains(searchTerm) || h.Email.Contains(searchTerm));
            }

            var totalRegistrations = await query.CountAsync();

            // Lấy dữ liệu với phân trang và tìm kiếm
            var pendingHosts = await query
                .Where(h => !h.IsApproved) // Chỉ lấy các đăng ký chưa được phê duyệt
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Tính số trang
            var totalPages = (int)Math.Ceiling((double)totalRegistrations / pageSize);

            // Thiết lập ViewBag cho phân trang và tìm kiếm
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRegistrations = totalRegistrations;
            ViewBag.SearchTerm = searchTerm;

            return View(pendingHosts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var registration = await _context.HostRegistrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound();
            }

            // Kiểm tra email của người dùng
            if (string.IsNullOrEmpty(registration.Email))
            {
                TempData["AlertMessage"] = "Email không tồn tại trong hệ thống";
                return View("PendingRegistrations"); // Hiển thị lại trang nếu email không tồn tại
            }

            // Tạo token phê duyệt
            var token = Guid.NewGuid().ToString();
            var tokenExpiry = DateTime.UtcNow.AddMinutes(10);
            _cache.Set(registration.Email, new { Token = token, Expiry = tokenExpiry }, tokenExpiry);

            _httpContextAccessor.HttpContext.Session.SetString("ApprovalToken", token);
            _httpContextAccessor.HttpContext.Session.SetString("Email", registration.Email);

            // Cập nhật trạng thái phê duyệt và lưu vào cơ sở dữ liệu
            registration.IsApproved = true;
            await _context.SaveChangesAsync();
            // Tìm người dùng và thay đổi role thành "3"
            //var user = await _userManager.FindByEmailAsync(registration.Email);
            //if (user != null)
            //{
            //    var roles = await _userManager.GetRolesAsync(user);
            //    // Xóa các roles cũ (nếu có)
            //    foreach (var role in roles)
            //    {
            //        await _userManager.RemoveFromRoleAsync(user, role);
            //    }

            //    // Thêm role mới với Id = 3
            //    await _userManager.AddToRolesAsync(user, "3"); // Thay "RoleNameForId3" bằng tên role thực tế
            //}
            // Thiết lập email
            var emailMessage = new MailMessage
            {
                From = new MailAddress("admin@gmail.com", "AdminBaotran"),
                Subject = "Phê duyệt đăng ký host",
                Body = $"Xin chào {registration.FullName},\n\nĐăng ký host của bạn đã được phê duyệt. Vui lòng sử dụng mã token sau: {token} để xác nhận. Token có hiệu lực trong 10 phút.",
                IsBodyHtml = false,
            };

            emailMessage.To.Add(new MailAddress(registration.Email));

            // Gửi email qua SMTP
            using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
            {
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential("tranthebaobt8@gmail.com", "kaiv zped dlrx xfgx");
                await smtpClient.SendMailAsync(emailMessage);
            }

            return RedirectToAction(nameof(PendingRegistrations)); // Quay lại trang danh sách đăng ký
        }

        // Action để từ chối đăng ký host
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var registration = await _context.HostRegistrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound();
            }

            // Xóa đăng ký host khỏi cơ sở dữ liệu
            _context.HostRegistrations.Remove(registration);
            await _context.SaveChangesAsync();

            // Gửi email thông báo từ chối
            var subject = "Đăng ký Host của bạn đã bị từ chối";
            var message = $"Xin chào {registration.FullName},<br/><br/>Rất tiếc, đăng ký host của bạn đã bị từ chối. Bạn có thể liên hệ với chúng tôi để biết thêm chi tiết.";

            // Thiết lập và gửi email từ chối
            var emailMessage = new MailMessage
            {
                From = new MailAddress("admin@gmail.com", "AdminBaotran"),
                Subject = subject,
                Body = message,
                IsBodyHtml = true,
            };

            emailMessage.To.Add(new MailAddress(registration.Email));

            using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
            {
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential("tranthebaobt8@gmail.com", "kaiv zped dlrx xfgx");
                await smtpClient.SendMailAsync(emailMessage);
            }

            // Chuyển hướng về trang danh sách đăng ký chờ xử lý
            return RedirectToAction(nameof(PendingRegistrations));
        }

    }
}
