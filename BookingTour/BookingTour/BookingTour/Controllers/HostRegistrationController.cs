using BookingTour.Data;
using BookingTour.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using System.Security.Claims;

namespace BookingTour.Controllers
{
    public class HostRegistrationController : Controller
    {
        private readonly YourExistingDbContextName _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public HostRegistrationController(YourExistingDbContextName context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var registrations = _context.HostRegistrations.ToList();
            return View(registrations);
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, string email)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Họ và tên và Email là bắt buộc.");
                return View(); // Trả về view nếu có lỗi
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                ModelState.AddModelError(string.Empty, "Bạn cần phải đăng nhập để thực hiện đăng ký.");
                return View(); // Trả về view nếu người dùng không đăng nhập
            }

            // Tạo mới một đối tượng HostRegistration
            var registration = new HostRegistration
            {
                UserId = currentUser.Id,
                FullName = fullName,
                Email = email,
                RegistrationDate = DateTime.Now,
                IsApproved = false // Mặc định là chưa được phê duyệt
            };

            try
            {
                _context.HostRegistrations.Add(registration); // Thêm mô hình vào cơ sở dữ liệu
                await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
                return RedirectToAction("Index", "Home"); // Chuyển hướng sau khi lưu thành công
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Có lỗi xảy ra khi lưu dữ liệu: {ex.Message}");
                return View("Index"); // Trả về view nếu có lỗi
            }
           
        }

    }
}
