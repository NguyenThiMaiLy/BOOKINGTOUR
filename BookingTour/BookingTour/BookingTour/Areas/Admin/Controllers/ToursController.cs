using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookingTour.Data;
using BookingTour.Models;
using Microsoft.AspNetCore.Authorization;

namespace BookingTour.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = CD.Role_Admin)]
    public class ToursController : Controller
    {
        private readonly YourExistingDbContextName _context;

        public ToursController(YourExistingDbContextName context)
        {
            _context = context;
        }

        // GET: Tours
        public async Task<IActionResult> Index(int page = 1, string searchTerm = "")
        {
            if (!User.IsInRole("ADMIN"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            const int pageSize = 10; // Số bản ghi mỗi trang

            // Lấy danh sách tour có trạng thái "Chờ Duyệt" và áp dụng từ khóa tìm kiếm nếu có
            var query = _context.Tours
                                .Include(t => t.IdHotelNavigation)
                                .Include(t => t.IdTransNavigation)
                                .Include(t => t.IdTypeNavigation)
                                .Where(t => t.ApprovalStatus == "Chờ Duyệt" &&
                                            (string.IsNullOrEmpty(searchTerm) || t.Name.Contains(searchTerm))); // Tìm kiếm theo tên tour

            // Tính tổng số bản ghi
            var totalTours = await query.CountAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling(totalTours / (double)pageSize);

            // Lấy các bản ghi cho trang hiện tại
            var tours = await query
                .OrderBy(t => t.IdTour) // Sắp xếp theo IdTour (hoặc thuộc tính khác nếu muốn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Gửi thông tin đến view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize; // Gửi pageSize đến view
            ViewBag.SearchTerm = searchTerm; // Gửi từ khóa tìm kiếm vào ViewBag

            return View(tours);
        }

        public IActionResult ApproveTour(long id)
        {
            var tour = _context.Tours.Find(id);
            if (tour == null) return NotFound();

            tour.ApprovalStatus = "Đã phê duyệt";
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Tour đã được phê duyệt thành công!";
            return RedirectToAction("Index", "Tours");
        }

        public IActionResult RejectTour(long id)
        {
            var tour = _context.Tours.Find(id);
            if (tour == null) return NotFound();

            tour.ApprovalStatus = "Từ chối";
            _context.SaveChanges();
            TempData["ErrorMessage"] = "Tour đã bị từ chối.";
            return RedirectToAction("Index", "Tours");
        }

        // GET: Tours/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours
                .Include(t => t.IdHotelNavigation)
                .Include(t => t.IdTransNavigation)
                .Include(t => t.IdTypeNavigation)
                .FirstOrDefaultAsync(m => m.IdTour == id);
            if (tour == null)
            {
                return NotFound();
            }

            return View(tour);
        }

        // GET: Tours/Create
        public IActionResult Create()
        {
            ViewData["IdHotel"] = new SelectList(_context.Hotels, "IdHotel", "IdHotel");
            ViewData["IdTrans"] = new SelectList(_context.Transportations, "IdTrans", "IdTrans");
            ViewData["IdType"] = new SelectList(_context.TypeOfTours, "IdType", "IdType");
            return View();
        }

        // POST: Tours/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdTour,Name,Description,Image,StartDate,EndDate,MaxQuantity,Price,IsDelete,IdType,IdTrans,IdHotel")] Tour tour)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tour);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdHotel"] = new SelectList(_context.Hotels, "IdHotel", "IdHotel", tour.IdHotel);
            ViewData["IdTrans"] = new SelectList(_context.Transportations, "IdTrans", "IdTrans", tour.IdTrans);
            ViewData["IdType"] = new SelectList(_context.TypeOfTours, "IdType", "IdType", tour.IdType);
            return View(tour);
        }

        // GET: Tours/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
            {
                return NotFound();
            }
            ViewData["IdHotel"] = new SelectList(_context.Hotels, "IdHotel", "IdHotel", tour.IdHotel);
            ViewData["IdTrans"] = new SelectList(_context.Transportations, "IdTrans", "IdTrans", tour.IdTrans);
            ViewData["IdType"] = new SelectList(_context.TypeOfTours, "IdType", "IdType", tour.IdType);
            return View(tour);
        }

        // POST: Tours/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("IdTour,Name,Description,Image,StartDate,EndDate,MaxQuantity,Price,IsDelete,IdType,IdTrans,IdHotel")] Tour tour)
        {
            if (id != tour.IdTour)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tour);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TourExists(tour.IdTour))
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
            ViewData["IdHotel"] = new SelectList(_context.Hotels, "IdHotel", "IdHotel", tour.IdHotel);
            ViewData["IdTrans"] = new SelectList(_context.Transportations, "IdTrans", "IdTrans", tour.IdTrans);
            ViewData["IdType"] = new SelectList(_context.TypeOfTours, "IdType", "IdType", tour.IdType);
            return View(tour);
        }

        // GET: Tours/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours
                .Include(t => t.IdHotelNavigation)
                .Include(t => t.IdTransNavigation)
                .Include(t => t.IdTypeNavigation)
                .FirstOrDefaultAsync(m => m.IdTour == id);
            if (tour == null)
            {
                return NotFound();
            }

            return View(tour);
        }

        // POST: Tours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour != null)
            {
                _context.Tours.Remove(tour);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TourExists(long id)
        {
            return _context.Tours.Any(e => e.IdTour == id);
        }
        private bool HasAccess(string role)
        {
            return User.IsInRole(role);
        }

        // Một action ví dụ
        public IActionResult SomeAction()
        {
            if (!HasAccess("Admin"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            // Thực hiện hành động nếu có quyền
            return View();
        }
    }
}
