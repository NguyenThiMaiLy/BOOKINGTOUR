using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookingTour.Data;
using BookingTour.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BookingTour.Controllers
{
    public class CommentsController : Controller
    {
        private readonly YourExistingDbContextName _context;
       
        public CommentsController(YourExistingDbContextName context)
        {
            _context = context;
         
        }

        // GET: Comments
        public async Task<IActionResult> Index()
        {
            var yourExistingDbContextName = _context.Comments.Include(c => c.IdNavigation).Include(c => c.IdTourNavigation);
            return View(await yourExistingDbContextName.ToListAsync());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> report(long idTour, string rating)
        {
            
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var report = new Report
                {
                    IdTour = idTour,
                    Rating = rating,
                    CreatedAt = DateTime.Now,
                    Id = user, // Giả sử User.Identity.Name là Id của người dùng hiện tại
                };
                report.IdReport = _context.Reports.Max(p => p.IdReport) + 1;
                // Lưu vào database
                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Báo cáo đã được gửi thành công.";
                return RedirectToAction("Details", "Tours", new { id = idTour });
            

          
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> favorite(long idTour)
        {
            if (ModelState.IsValid)
            {
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var favorite = new UsersFavoriteTour
                {
                    IdTour = idTour,
                  
                    Id = user, // Giả sử User.Identity.Name là Id của người dùng hiện tại
                };
              
                // Lưu vào database
                _context.UsersFavoriteTours.Add(favorite);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Lưu thành công.";
                return RedirectToAction("Details", "Tours", new { id = idTour });
            }

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unfavorite(long idTour)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Tìm bản ghi yêu thích mà khớp với IdTour và Id của người dùng
                var favorite = await _context.UsersFavoriteTours
                    .FirstOrDefaultAsync(f => f.IdTour == idTour && f.Id == userId);

                if (favorite != null)
                {
                    // Xóa bản ghi yêu thích
                    _context.UsersFavoriteTours.Remove(favorite);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Đã hủy yêu thích tour.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Tour này không phải là yêu thích của bạn.";
                }

                return RedirectToAction("Details", "Tours", new { id = idTour });
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(string? Content, long IdTour, int? Rating)
        {
            
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    // Kiểm tra xem người dùng đã bình luận cho tour này chưa
                    var existingReview = await _context.Comments
                        .FirstOrDefaultAsync(r => r.IdTour == IdTour && r.Id == userId);

                    if (existingReview != null)
                    {
                    // Nếu đã có bình luận, thông báo cho người dùng và không lưu bình luận mới
                    TempData["SuccessMessage"] = "Chỉ được bình luận 1 lần!";
                    return RedirectToAction("Details", "Tours", new { id = IdTour });
                    }

                    // Nếu chưa có bình luận, tạo bình luận mới
                    var review = new Comment
                    {
                        IdTour = IdTour,
                        Id = userId,
                        Content = Content,
                        Rating = Rating, // Lưu giá trị rating vào Comment
                        CommentDate = DateTime.Now,
                        IsHidden = false  // Có thể thiết lập logic ẩn hiện sau khi bình luận
                    };

                    _context.Comments.Add(review);
                    await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bình luận của bạn đã được gửi thành công!";
                return RedirectToAction("Details", "Tours", new { id = IdTour });
                }
            

            return View(); // Trả về form nếu có lỗi
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.IdNavigation)
                .Include(c => c.IdTourNavigation)
                .FirstOrDefaultAsync(m => m.IdTour == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            ViewData["Id"] = new SelectList(_context.AspNetUsers, "Id", "Id");
            ViewData["IdTour"] = new SelectList(_context.Tours, "IdTour", "IdTour");
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdTour,Id,Content,CommentDate,Reply")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.AspNetUsers, "Id", "Id", comment.Id);
            ViewData["IdTour"] = new SelectList(_context.Tours, "IdTour", "IdTour", comment.IdTour);
            return View(comment);
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.AspNetUsers, "Id", "Id", comment.Id);
            ViewData["IdTour"] = new SelectList(_context.Tours, "IdTour", "IdTour", comment.IdTour);
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("IdTour,Id,Content,CommentDate,Reply")] Comment comment)
        {
            if (id != comment.IdTour)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(comment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.IdTour))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.AspNetUsers, "Id", "Id", comment.Id);
            ViewData["IdTour"] = new SelectList(_context.Tours, "IdTour", "IdTour", comment.IdTour);
            return View(comment);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.IdNavigation)
                .Include(c => c.IdTourNavigation)
                .FirstOrDefaultAsync(m => m.IdTour == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommentExists(long id)
        {
            return _context.Comments.Any(e => e.IdTour == id);
        }
    }
}
