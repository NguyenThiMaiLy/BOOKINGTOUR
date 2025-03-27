using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookingTour.Data;
using BookingTour.Models;
using SQLitePCL;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using System.Net;

namespace BookingTour.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PromotionsController : Controller
    {
        private readonly YourExistingDbContextName _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public PromotionsController(YourExistingDbContextName context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Promotions
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string searchTerm = "")
        {
            // Truy vấn bảng Promotions và áp dụng tìm kiếm nếu có từ khóa
            var query = _context.Promotions.AsQueryable();

            // Nếu có từ khóa tìm kiếm, lọc theo tên khuyến mãi hoặc các thuộc tính khác
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm)); // Tìm theo tên khuyến mãi
            }

            // Tính tổng số bản ghi sau khi lọc
            var totalPromotions = await query.CountAsync();

            // Tính số trang
            var totalPages = (int)Math.Ceiling((double)totalPromotions / pageSize);

            // Lấy dữ liệu với phân trang
            var promotions = await query
                .Include(p => p.IdHotelNavigation)
                .Include(p => p.IdTourNavigation)
                .OrderBy(p => p.StartDate) // Sắp xếp theo ngày bắt đầu (hoặc thuộc tính bạn muốn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Thiết lập ViewBag cho phân trang và tìm kiếm
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = searchTerm; // Gửi từ khóa tìm kiếm vào ViewBag

            return View(promotions);
        }

        // GET: Admin/Promotions/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promotion = await _context.Promotions
                .Include(p => p.IdHotelNavigation)
                .Include(p => p.IdTourNavigation)
                .FirstOrDefaultAsync(m => m.IdPmt == id);
            if (promotion == null)
            {
                return NotFound();
            }

            return View(promotion);
        }

        // GET: Admin/Promotions/Create
        public IActionResult Create()
        {
            ViewData["IdHotel"] = new SelectList(_context.Hotels, "IdHotel", "IdHotel");
            ViewData["IdTour"] = new SelectList(_context.Tours, "IdTour", "IdTour");
            return View();
        }

        // POST: Admin/Promotions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Admin/Promotions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,StartDate,EndDate,DiscountAmount,EligibilityCriteria,Status,IdTour,IdHotel")] Promotion promotion)
        {
            ViewData["IdTour"] = new SelectList(await _context.Tours.ToListAsync(), "IdTour", "Name");
            ViewData["IdHotel"] = new SelectList(await _context.Hotels.ToListAsync(), "IdHotel", "Name");
            // Tạo một đối tượng Promotion mới
            Promotion newPromotion = new Promotion
                {

                    Name = promotion.Name,
                    Description = promotion.Description,
                    StartDate = promotion.StartDate,
                    EndDate = promotion.EndDate,
                    DiscountAmount = promotion.DiscountAmount,
                    EligibilityCriteria = promotion.EligibilityCriteria,
                    Status = promotion.Status,
                    IdTour = promotion.IdTour,
                    IdHotel = promotion.IdHotel
                };
            newPromotion.IdPmt = _context.Promotions.Max(p => p.IdPmt) + 1;
            // Thêm vào cơ sở dữ liệu
            _context.Add(newPromotion);
                await _context.SaveChangesAsync();

                // Gửi email thông báo đến tất cả người dùng
                var users = await _userManager.Users.ToListAsync(); // Lấy tất cả người dùng
                var subject = "Thông báo về chương trình khuyến mãi mới";
                var body = $"Chúng tôi vui mừng thông báo rằng một chương trình khuyến mãi mới đã được thêm vào hệ thống: {promotion.Name}\n\n" +
                           $"Chương trình này sẽ bắt đầu từ {promotion.StartDate} và kết thúc vào {promotion.EndDate}.\n" +
                           $"Giảm giá: {promotion.DiscountAmount}%.\n\n" +
                           "Hãy tận dụng cơ hội này và đăng ký ngay bây giờ!";

                foreach (var user in users)
                {
                    // Kiểm tra xem người dùng có email không
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        // Tạo email
                        var emailMessage = new MailMessage
                        {
                            From = new MailAddress("admin@gmail.com", "AdminBaotran"),
                            Subject = subject,
                            Body = body,
                            IsBodyHtml = false,
                        };

                        emailMessage.To.Add(new MailAddress(user.Email));

                        // Gửi email qua SMTP
                        using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                        {
                            smtpClient.EnableSsl = true;
                            smtpClient.Credentials = new NetworkCredential("tranthebaobt8@gmail.com", "kaiv zped dlrx xfgx"); // Thay đổi mật khẩu phù hợp
                            await smtpClient.SendMailAsync(emailMessage);
                        }
                    }
                }

                return RedirectToAction(nameof(Index)); // Chuyển hướng về danh sách sau khi tạo thành công
            

            // Nếu ModelState không hợp lệ, hiển thị lại view với thông tin đã nhập
            ViewData["IdHotel"] = new SelectList(_context.Hotels, "IdHotel", "Name", promotion.IdHotel);
            ViewData["IdTour"] = new SelectList(_context.Tours, "IdTour", "Name", promotion.IdTour);
            return View(promotion);
        }



        // GET: Admin/Promotions/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promotion = await _context.Promotions
                .Include(p => p.IdHotelNavigation)
                .Include(p => p.IdTourNavigation)
                .FirstOrDefaultAsync(p => p.IdPmt == id);

            if (promotion == null)
            {
                return NotFound();
            }

            // Gửi danh sách Hotels và Tours cho view để hiển thị trong dropdown
            ViewData["IdHotel"] = new SelectList(_context.Hotels, "IdHotel", "HotelName", promotion.IdHotel);
            ViewData["IdTour"] = new SelectList(_context.Tours, "IdTour", "TourName", promotion.IdTour);
            return View(promotion);
        }

        // POST: Promotion/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit1(long id, [Bind("IdPmt, Name, DiscountAmount, EligibilityCriteria")] Promotion promotion)
        {
            if (id != promotion.IdPmt)
            {
                TempData["SuccessMessage"] = "id ko khop!";
                return RedirectToAction(nameof(Index));
            }

           
                try
                {
                    var promotionToUpdate = await _context.Promotions.FindAsync(id);
                    if (promotionToUpdate == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các thuộc tính cần thiết
                    promotionToUpdate.Name = promotion.Name;
                    promotionToUpdate.DiscountAmount = promotion.DiscountAmount;
                    promotionToUpdate.EligibilityCriteria = promotion.EligibilityCriteria;

                    // Lưu thay đổi vào cơ sở dữ liệu
                    _context.Update(promotionToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PromotionExists(promotion.IdPmt))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
           
        }

        // GET: Admin/Promotions/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            // Kiểm tra xem id có null hay không
            if (id == null)
            {
                return NotFound(); // Trả về NotFound nếu không có id
            }

            // Tìm khuyến mãi theo id
            var promotion = await _context.Promotions
                .Include(p => p.IdHotelNavigation) // Bao gồm thông tin khách sạn nếu cần
                .Include(p => p.IdTourNavigation) // Bao gồm thông tin tour nếu cần
                .FirstOrDefaultAsync(m => m.IdPmt == id); // Tìm khuyến mãi đầu tiên có IdPmt bằng id

            // Kiểm tra xem khuyến mãi có tồn tại không
            if (promotion == null)
            {
                return NotFound(); // Trả về NotFound nếu không tìm thấy khuyến mãi
            }

            return View(promotion); // Trả về view để hiển thị thông tin khuyến mãi
        }


        // POST: Admin/Promotions/Delete/5
        // POST: Admin/Promotions/Delete/5
        [HttpPost]
      
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            // Tìm khuyến mãi theo id
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion != null)
            {
                // Tìm tất cả các bản ghi trong bảng PROMOTION_USER có ID_PMT trùng với khuyến mãi
                var relatedPromotions = await _context.Promotions
                    .Where(pu => pu.IdPmt == id) // Lọc theo ID_PMT
                    .ToListAsync();

                // Nếu tìm thấy các bản ghi liên quan, xóa chúng
                if (relatedPromotions.Any())
                {
                    _context.Promotions.RemoveRange(relatedPromotions); // Xóa các bản ghi liên quan
                }

                // Cuối cùng, xóa khuyến mãi
                _context.Promotions.Remove(promotion);
                await _context.SaveChangesAsync(); // Lưu các thay đổi vào cơ sở dữ liệu
            }
            TempData["SuccessMessage"] = "Xóa thành công!";
            return RedirectToAction(nameof(Index)); // Chuyển hướng về trang danh sách
        }



        private bool PromotionExists(long id)
        {
            return _context.Promotions.Any(e => e.IdPmt == id);
        }
    }
}
