using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookingTour.Data;
using BookingTour.Models;
using System.Security.Claims;

namespace BookingTour.Areas.Host.Controllers
{
    [Area("Host")]
    public class CommentsController : Controller
    {
        private readonly YourExistingDbContextName _context;

        public CommentsController(YourExistingDbContextName context)
        {
            _context = context;
        }

        // GET: Host/Comments
        public async Task<IActionResult> Index(long idTour, int page = 1, string searchTerm = "")
        {
            // Lấy Id của người dùng hiện tại từ User.Identity.Name
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Người dùng chưa đăng nhập!";
                return RedirectToAction("Login", "Account");
            }

            // Xác định số lượng bình luận trên mỗi trang
            int pageSize = 10;

            // Lấy tổng số bình luận cho tour và người dùng hiện tại, áp dụng tìm kiếm nếu có
            var totalCommentsQuery = _context.Comments
                .Where(c => c.IdTourNavigation.IdUser == userId &&
                            (string.IsNullOrEmpty(searchTerm) || c.Content.Contains(searchTerm))); // Tìm kiếm theo nội dung bình luận

            var totalComments = await totalCommentsQuery.CountAsync();

            // Tính số trang
            var totalPages = (int)Math.Ceiling(totalComments / (double)pageSize);

            // Lấy danh sách bình luận theo trang, áp dụng tìm kiếm nếu có
            var comments = await _context.Comments
                .Where(c => c.IdTourNavigation.IdUser == userId &&
                            (string.IsNullOrEmpty(searchTerm) || c.Content.Contains(searchTerm))) // Tìm kiếm theo nội dung bình luận
                .Include(c => c.IdNavigation)
                .Include(c => c.IdTourNavigation)
                .OrderBy(c => c.CommentDate) // Sắp xếp theo ngày bình luận
                .Skip((page - 1) * pageSize) // Bỏ qua các bình luận đã hiển thị ở các trang trước
                .Take(pageSize) // Lấy số lượng bình luận của trang hiện tại
                .ToListAsync();

            // Trả về view với thông tin phân trang và tìm kiếm
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = searchTerm;  // Truyền từ khóa tìm kiếm về view
            ViewBag.IdTour = idTour;  // Truyền IdTour vào ViewBag

            return View(comments);
        }


        // Phản hồi bình luận
        [HttpPost]
        public async Task<IActionResult> Reply(long idTour, string idComment, string replyContent)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(replyContent))
            {
                // Bạn có thể trả về lỗi hoặc một thông báo nếu không có nội dung trả lời
                ModelState.AddModelError("", "Nội dung trả lời không được để trống.");
                return RedirectToAction("Index", new { idTour });
            }

            // Tìm bình luận theo idComment và idTour (cả 2 đều là khóa trong cơ sở dữ liệu)
            var comment = await _context.Comments
                                         .FirstOrDefaultAsync(c => c.Id == idComment && c.IdTour == idTour);

            if (comment != null)
            {
                // Cập nhật phản hồi cho bình luận
                comment.Reply = replyContent;
                await _context.SaveChangesAsync();
            }
            else
            {
                // Nếu không tìm thấy comment, có thể xử lý thêm (ví dụ, trả về thông báo lỗi)
                ModelState.AddModelError("", "Bình luận không tồn tại.");
                return RedirectToAction("Index", new { idTour });
            }

            // Sau khi cập nhật xong, trả về trang danh sách các bình luận của tour
            return RedirectToAction("Index", new { idTour });
        }



        // Ẩn bình luận
        [HttpPost]
        public async Task<IActionResult> Hide(long idTour, string idComment, bool isHidden)
        {
            var comment = await _context.Comments
              .FirstOrDefaultAsync(c => c.IdTour == idTour && c.Id == idComment);

            if (comment == null)
            {
                return NotFound();  // Nếu không tìm thấy comment, trả về lỗi 404
            }

            // Cập nhật trạng thái IsHidden
            comment.IsHidden = true;

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Điều hướng về trang danh sách comment hoặc trang khác nếu cần
            return RedirectToAction(nameof(Index), new { idTour });
        }

        // Cập nhật điểm đánh giá (Rating)
        public async Task<IActionResult> UpdateRating(long idTour, string idComment, int rating)
        {
            // Tìm comment dựa trên composite key (IdTour và Id)
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.IdTour == idTour && c.Id == idComment);

            if (comment == null)
            {
                return NotFound();  // Nếu không tìm thấy comment, trả về lỗi 404
            }

            // Cập nhật rating
            comment.Rating = rating;

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Điều hướng về trang danh sách comment (hoặc trang nào khác tùy nhu cầu)
            return RedirectToAction(nameof(Index), new { idTour });
        }
        // GET: Host/Comments/Details/5
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

        // GET: Host/Comments/Create
        public IActionResult Create()
        {
            ViewData["Id"] = new SelectList(_context.AspNetUsers, "Id", "Id");
            ViewData["IdTour"] = new SelectList(_context.Tours, "IdTour", "IdTour");
            return View();
        }

        // POST: Host/Comments/Create
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

        // GET: Host/Comments/Edit/5
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

        // POST: Host/Comments/Edit/5
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

        // GET: Host/Comments/Delete/5
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

        // POST: Host/Comments/Delete/5
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
