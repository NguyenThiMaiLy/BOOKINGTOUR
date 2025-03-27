// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using BookingTour.Models;
using Google.Rpc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;

namespace BookingTour.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string Code { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string Code { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string code = null, string email = null)
        {
            if (code == null || email == null)
            {
                return BadRequest("Mã token hoặc email không hợp lệ.");
            }

            Code = code;
            Input = new InputModel { Email = email, Code = code };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Bảo mật thông tin - không thông báo nếu người dùng không tồn tại
                return RedirectToPage("/Account/Register", new { area = "Identity" });
            }

            // Giải mã token từ URL
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(Input.Code);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            // Đặt lại mật khẩu
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, Input.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }
            TempData["LockoutMessage"] = "Đổi mật khẩu thành công";
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }
    }

}
