// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BookingTour.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace BookingTour.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
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
        // Bảo mật thông tin - không thông báo người dùng không tồn tại
        return RedirectToPage("./ForgotPasswordConfirmation");
    }

    // Tạo token đặt lại mật khẩu
    var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
    var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

    // Tạo URL cho đặt lại mật khẩu
    var resetPasswordUrl = Url.Page(
        "/Account/ResetPassword",
        pageHandler: null,
        values: new { area = "Identity", code = encodedToken, email = Input.Email },
        protocol: Request.Scheme);

    // Tạo email
    var emailMessage = new MailMessage
    {
        From = new MailAddress("tranthebaobt8@gmail.com", "AdminBaotran"), // Thay đổi địa chỉ email và tên
        Subject = "Đặt lại mật khẩu",
        Body = $"Click vào link để đặt lại mật khẩu: <a href='{resetPasswordUrl}'>đặt lại mật khẩu</a>",
        IsBodyHtml = true, // Cho phép gửi email dưới dạng HTML
    };

    emailMessage.To.Add(new MailAddress(Input.Email));

    // Gửi email qua SMTP
    using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
    {
        smtpClient.EnableSsl = true;
        smtpClient.Credentials = new NetworkCredential("tranthebaobt8@gmail.com", "kaiv zped dlrx xfgx"); // Thay đổi mật khẩu ứng dụng của bạn
        await smtpClient.SendMailAsync(emailMessage);
    }

    return RedirectToPage("/Account/ResetPassword", new { area = "Identity", code = encodedToken, email = Input.Email });
}

    }
}
