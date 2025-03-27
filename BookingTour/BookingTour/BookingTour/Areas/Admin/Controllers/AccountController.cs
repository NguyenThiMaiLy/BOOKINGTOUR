using BookingTour.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Claims;

namespace BookingTour.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SmsService _smsService;
        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, SmsService smsService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _smsService = smsService;
        }
            [HttpGet]
            public IActionResult SendOtp()
            {
                return View();
            }

            //[HttpPost]
            //public async Task<IActionResult> SendOtp(string phoneNumber)
            //{
            //    if (string.IsNullOrWhiteSpace(phoneNumber))
            //    {
            //        ModelState.AddModelError("", "Số điện thoại không được để trống.");
            //        return View(); // Quay lại view với lỗi
            //    }

            //    var user = await _userManager.FindByNameAsync(phoneNumber);
            //    if (user == null)
            //    {
            //        ModelState.AddModelError("", "User không tồn tại.");
            //        return View(); // Quay lại view với lỗi
            //    }

            //    // Tạo mã OTP ngẫu nhiên
            //    var otp = new Random().Next(100000, 999999).ToString();
            //    user.Otp = otp;
            //    user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(5);
            //    await _userManager.UpdateAsync(user);

            //    // Gửi OTP qua SMS
            //    await _smsService.SendSmsAsync(phoneNumber, $"Your OTP code is {otp}");

            //    return RedirectToAction("VerifyOtp", new { phoneNumber });
            //}

            [HttpGet]
            public IActionResult VerifyOtp(string phoneNumber)
            {
                return View(new VerifyOtpViewModel { PhoneNumber = phoneNumber });
            }

            //[HttpPost]
            //public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
            //{
            //    if (ModelState.IsValid)
            //    {
            //        var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            //        if (user == null || user.Otp != model.Otp || user.OtpExpiryTime < DateTime.UtcNow)
            //        {
            //            ModelState.AddModelError("", "OTP không hợp lệ hoặc đã hết hạn.");
            //            return View(model); // Quay lại view với lỗi
            //        }

            //        await _signInManager.SignInAsync(user, isPersistent: false);
            //        user.Otp = null; // Xóa OTP sau khi xác minh
            //        user.OtpExpiryTime = DateTime.MinValue;
            //        await _userManager.UpdateAsync(user);
            //        return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chính
            //    }

            //    return View(model); // Quay lại view với lỗi
            //}

        
         


        
    [HttpGet]
        public IActionResult LoginWithGoogle(string returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Challenge(properties, "Google");
        }
        [HttpGet]
        public IActionResult LoginWithFacebook(string returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Facebook", redirectUrl);
            return Challenge(properties, "Facebook");
        }
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Index", "Tours");
            }

            // Kiểm tra xem tài khoản đã tồn tại không
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                // Nếu đăng nhập thành công, chuyển đến trang chính
                return RedirectToAction("Index", "Tours");
            }
            else
            {
                // Tạo tài khoản mới chỉ khi không tìm thấy tài khoản
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var user = await _userManager.FindByEmailAsync(email);

                if (user != null) // Nếu tài khoản đã tồn tại, đăng nhập tài khoản đó
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Tours");
                }
                else // Nếu tài khoản không tồn tại, tạo tài khoản mới
                {
                    var fullName = info.Principal.FindFirstValue(ClaimTypes.Name);
                    user = new ApplicationUser { UserName = email, Email = email, fullname = fullName };

                    var createResult = await _userManager.CreateAsync(user);
                    if (createResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error creating user.");
                        return RedirectToAction("Index1", "Home");
                    }
                }
            }

        }

    }

}