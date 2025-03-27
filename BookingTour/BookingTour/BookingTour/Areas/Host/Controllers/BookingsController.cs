using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookingTour.Data;
using BookingTour.Models;

namespace BookingTour.Areas.Host.Controllers
{
    [Area("Host")]
    public class BookingsController : Controller
    {
        private readonly YourExistingDbContextName _context;

        public BookingsController(YourExistingDbContextName context)
        {
            _context = context;
        }
        public async Task<IActionResult> PendingBookings(int page = 1, string searchTerm = "")
        {
            // Xác định số lượng booking trên mỗi trang
            int pageSize = 10;

            // Tính tổng số booking có trạng thái là "Đang chờ", áp dụng tìm kiếm nếu có
            var totalBookingsQuery = _context.Bookings
                .Include(b => b.IdHotelNavigation)   // Lấy thông tin khách sạn
                .Include(b => b.IdTourNavigation)    // Lấy thông tin tour
                .Include(b => b.IdStatusNavigation)  // Lấy thông tin trạng thái booking
                .Where(b => b.IdStatus == 4 &&
                           (string.IsNullOrEmpty(searchTerm) || b.IdTourNavigation.Name.Contains(searchTerm)));

    // Tính tổng số booking
    var totalBookings = await totalBookingsQuery.CountAsync();

            // Tính số trang
            var totalPages = (int)Math.Ceiling(totalBookings / (double)pageSize);

            // Lấy danh sách booking theo trang, áp dụng tìm kiếm nếu có
            var bookings = await _context.Bookings
                .Include(b => b.IdHotelNavigation)   // Lấy thông tin khách sạn
                .Include(b => b.IdTourNavigation)    // Lấy thông tin tour
                .Include(b => b.IdStatusNavigation)  // Lấy thông tin trạng thái booking
                .Where(b => b.IdStatus == 4 &&
                           (string.IsNullOrEmpty(searchTerm) || b.IdTourNavigation.Name.Contains(searchTerm))) // Tìm kiếm theo tên tour
                .OrderBy(b => b.BookingTime) // Sắp xếp theo thời gian đặt tour
                .Skip((page - 1) * pageSize) // Bỏ qua các booking ở các trang trước
                .Take(pageSize) // Lấy số lượng booking của trang hiện tại
                .ToListAsync();

            // Tìm kiếm FullName trong bảng AspNetUsers dựa trên UserId trong bảng Bookings
            var userIds = bookings.Select(b => b.Id).Distinct();  // Lấy danh sách UserId duy nhất từ danh sách bookings
            var users = await _context.AspNetUsers
                .Where(u => userIds.Contains(u.Id)) // Lọc những người dùng có UserId trong danh sách
                .Select(u => new { u.Id, u.fullname }) // Chỉ lấy Id và FullName
                .ToListAsync();

            // Đưa danh sách FullName vào ViewBag
            ViewBag.UserFullNames = users.ToDictionary(u => u.Id, u => u.fullname);  // Lưu vào ViewBag theo kiểu từ điển UserId -> FullName

            // Truyền thông tin phân trang và tìm kiếm vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = searchTerm;  // Truyền từ khóa tìm kiếm về view

            return View(bookings);
        }


        public async Task<IActionResult> ApproveBooking(long id)
        {
            var booking = await _context.Bookings
                .Include(b => b.IdStatusNavigation)  // Lấy thông tin trạng thái booking
                .FirstOrDefaultAsync(b => b.IdBooking == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái đặt chỗ thành "Đã phê duyệt" (Ví dụ, 2 là trạng thái "Đã phê duyệt")
            booking.IdStatus = 1;
            await _context.SaveChangesAsync();

            // Có thể thêm email hoặc thông báo nếu cần
            return RedirectToAction(nameof(PendingBookings)); // Trở lại danh sách các yêu cầu đặt chỗ
        }

        // Từ chối đặt chỗ
        public async Task<IActionResult> RejectBooking(long id)
        {
            var booking = await _context.Bookings
                .Include(b => b.IdStatusNavigation)
                .FirstOrDefaultAsync(b => b.IdBooking == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái đặt chỗ thành "Từ chối" (Ví dụ, 3 là trạng thái "Từ chối")
            booking.IdStatus = 3;
            await _context.SaveChangesAsync();

            // Có thể thêm email hoặc thông báo nếu cần
            return RedirectToAction(nameof(PendingBookings)); // Trở lại danh sách các yêu cầu đặt chỗ
        }
        // GET: Host/Bookings
        public async Task<IActionResult> Index()
        {
            var yourExistingDbContextName = _context.Bookings.Include(b => b.IdHotelNavigation).Include(b => b.IdNavigation).Include(b => b.IdStatusNavigation).Include(b => b.IdTourNavigation);
            return View(await yourExistingDbContextName.ToListAsync());
        }

        // GET: Host/Bookings/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.IdHotelNavigation)
                .Include(b => b.IdNavigation)
                .Include(b => b.IdStatusNavigation)
                .Include(b => b.IdTourNavigation)
                .FirstOrDefaultAsync(m => m.IdBooking == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Host/Bookings/Create
        public IActionResult Create()
        {
            ViewData["IdHotel"] = new SelectList(_context.Hotels, "IdHotel", "IdHotel");
            ViewData["Id"] = new SelectList(_context.AspNetUsers, "Id", "Id");
            ViewData["IdStatus"] = new SelectList(_context.BookingStatuses, "IdStatus", "IdStatus");
            ViewData["IdTour"] = new SelectList(_context.Tours, "IdTour", "IdTour");
            return View();
        }

        // POST: Host/Bookings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdBooking,CheckInDate,CheckOutDate,BookingTime,Id,IdHotel,IdTour,IdStatus")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdHotel"] = new SelectList(_context.Hotels, "IdHotel", "IdHotel", booking.IdHotel);
            ViewData["Id"] = new SelectList(_context.AspNetUsers, "Id", "Id", booking.Id);
            ViewData["IdStatus"] = new SelectList(_context.BookingStatuses, "IdStatus", "IdStatus", booking.IdStatus);
            ViewData["IdTour"] = new SelectList(_context.Tours, "IdTour", "IdTour", booking.IdTour);
            return View(booking);
        }

        // GET: Host/Bookings/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["IdHotel"] = new SelectList(_context.Hotels, "IdHotel", "IdHotel", booking.IdHotel);
            ViewData["Id"] = new SelectList(_context.AspNetUsers, "Id", "Id", booking.Id);
            ViewData["IdStatus"] = new SelectList(_context.BookingStatuses, "IdStatus", "IdStatus", booking.IdStatus);
            ViewData["IdTour"] = new SelectList(_context.Tours, "IdTour", "IdTour", booking.IdTour);
            return View(booking);
        }

        // POST: Host/Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("IdBooking,CheckInDate,CheckOutDate,BookingTime,Id,IdHotel,IdTour,IdStatus")] Booking booking)
        {
            if (id != booking.IdBooking)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.IdBooking))
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
            ViewData["IdHotel"] = new SelectList(_context.Hotels, "IdHotel", "IdHotel", booking.IdHotel);
            ViewData["Id"] = new SelectList(_context.AspNetUsers, "Id", "Id", booking.Id);
            ViewData["IdStatus"] = new SelectList(_context.BookingStatuses, "IdStatus", "IdStatus", booking.IdStatus);
            ViewData["IdTour"] = new SelectList(_context.Tours, "IdTour", "IdTour", booking.IdTour);
            return View(booking);
        }

        // GET: Host/Bookings/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.IdHotelNavigation)
                .Include(b => b.IdNavigation)
                .Include(b => b.IdStatusNavigation)
                .Include(b => b.IdTourNavigation)
                .FirstOrDefaultAsync(m => m.IdBooking == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Host/Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(long id)
        {
            return _context.Bookings.Any(e => e.IdBooking == id);
        }
    }
}
