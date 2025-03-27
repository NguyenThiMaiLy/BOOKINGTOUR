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
using System.Text;


namespace BookingTour.Areas.Admin.Controllers
{
    [Area("ADMIN")]
    [Authorize(Roles = CD.Role_Admin)]
    public class BookingStatusController : Controller
    {
        private readonly YourExistingDbContextName _context;
        private readonly ApplicationDbContext _context1;

        public BookingStatusController(YourExistingDbContextName context, ApplicationDbContext context1)
        {
            _context = context;
            _context1 = context1;
        }

        // GET: BookingStatus
        public async Task<IActionResult> Index(int? page, string searchTerm = "")
        {
            var pageSize = 10; // Số lượng booking mỗi trang
            var pageNumber = page ?? 1; // Nếu không có page thì mặc định là 1

            // Truy vấn các booking cùng thông tin liên quan, áp dụng tìm kiếm nếu có
            var query = from booking in _context.Bookings
                        join status in _context.BookingStatuses on booking.IdStatus equals status.IdStatus
                        join tour in _context.Tours on booking.IdTour equals tour.IdTour
                        join user in _context.AspNetUsers on booking.Id equals user.Id
                        where string.IsNullOrEmpty(searchTerm) || user.UserName.Contains(searchTerm) || tour.Name.Contains(searchTerm) // Tìm kiếm theo tên khách hàng và tên tour
                        select new BookingViewModel
                        {
                            CustomerName = user.UserName,
                            TourName = tour.Name,
                            BookingDate = booking.BookingTime ?? DateTime.Now,
                            StatusName = status.StatusName,
                            IdStatus = status.IdStatus
                        };

            var totalCount = await query.CountAsync(); // Tổng số booking sau khi tìm kiếm
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize); // Tính tổng số trang

            // Lấy các booking theo trang
            var pagedBookings = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Thiết lập các giá trị ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.SearchTerm = searchTerm; // Gửi từ khóa tìm kiếm vào ViewBag
            ViewData["DeleteMessage"] = TempData["DeleteMessage"];
            return View(pagedBookings); // Trả về các booking đã phân trang
        }


        // GET: BookingStatus/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookingStatus = await _context.BookingStatuses
                .FirstOrDefaultAsync(m => m.IdStatus == id);
            if (bookingStatus == null)
            {
                return NotFound();
            }

            return View(bookingStatus);
        }

        // GET: BookingStatus/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BookingStatus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdStatus,StatusName,Description,CreatedAt")] BookingStatus bookingStatus)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bookingStatus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bookingStatus);
        }

        // GET: BookingStatus/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookingStatus = await _context.BookingStatuses.FindAsync(id);
            if (bookingStatus == null)
            {
                return NotFound();
            }
            return View(bookingStatus);
        }

        // POST: BookingStatus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("IdStatus,StatusName,Description,CreatedAt")] BookingStatus bookingStatus)
        {
            if (id != bookingStatus.IdStatus)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookingStatus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingStatusExists(bookingStatus.IdStatus))
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
            return View(bookingStatus);
        }
        [HttpGet]
        // GET: BookingStatus/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookingStatus = await _context.BookingStatuses
                .FirstOrDefaultAsync(m => m.IdStatus == id);
            if (bookingStatus == null)
            {
                return NotFound();
            }

            return View(bookingStatus);
        }

        // POST: BookingStatus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var bookings = await _context.Bookings.Where(b => b.IdStatus == id).ToListAsync();
            if (bookings.Any())
            {
                _context.Bookings.RemoveRange(bookings);
            }
            var bookingStatus = await _context.BookingStatuses.FindAsync(id);
            if (bookingStatus != null)
            {
                _context.BookingStatuses.Remove(bookingStatus);
            }

            await _context.SaveChangesAsync();
            TempData["DeleteMessage"] = "Trạng thái đặt tour đã được xóa thành công!";
            return RedirectToAction(nameof(Index));
        }
        //[HttpGet]
        //public IActionResult ExportBookingStatistics()
        //{
        //    var statistics = GetBookingStatistics(); // Lấy dữ liệu thống kê
        //    var csv = new StringBuilder();

        //    // Thêm tiêu đề cột
        //    csv.AppendLine("Tháng,Tổng Doanh Thu,Số Lượt Đặt");

        //    // Thêm dữ liệu vào file CSV
        //    foreach (var item in statistics)
        //    {
        //        csv.AppendLine($"{item.Month},{item.TotalRevenue},{item.TotalBookings}");
        //    }

        //    // Đặt tên file
        //    var fileName = $"BookingStatistics_{DateTime.Now:yyyyMMddHHmmss}.csv";
        //    return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        //}


        public IActionResult BookingStatistics()
        {
            // Thống kê doanh thu và số lượt đặt tour theo các khoảng thời gian
            var statistics = GetBookingStatistics();

            // Lấy top tour được đặt nhiều nhất
            var topBookedTours = GetTopBookedTours();

            // Gửi dữ liệu đến view
            ViewBag.TopBookedTours = topBookedTours; // Truyền dữ liệu tour vào ViewBag
            return View(statistics);
        }
        private List<object> GetTopBookedTours()
        {
            var topTours = _context.Bookings
                .GroupBy(b => b.IdTour)
                .Select(g => new
                {
                    TourName = g.FirstOrDefault().IdTourNavigation.Name, // Lấy tên tour
                    TotalBookings = g.Count()
                })
                .OrderByDescending(g => g.TotalBookings)
                .Take(5)
                .ToList();

            return topTours.Select(t => new { t.TourName, t.TotalBookings }).ToList<object>();
        }
        private List<object> GetBookingStatistics()
        {
            var data = _context.Bookings
                .GroupBy(b => b.BookingTime.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TotalRevenue = g.Sum(b => b.IdTourNavigation.Price ), // Ví dụ tổng doanh thu
                    TotalBookings = g.Count() // Số lượt đặt tour
                }).ToList();

            return data.Select(d => new { d.Month, d.TotalRevenue, d.TotalBookings }).ToList<object>();
        }
        private bool BookingStatusExists(long id)
        {
            return _context.BookingStatuses.Any(e => e.IdStatus == id);
        }
    }
}
