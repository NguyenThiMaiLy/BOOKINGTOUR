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

namespace BookingTour.Controllers
{
    public class HistoryController : Controller
    {
        private readonly YourExistingDbContextName _context;

        public HistoryController(YourExistingDbContextName context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy ID người dùng hiện tại

            // Tính toán số bản ghi cần lấy từ database dựa trên trang hiện tại
            var skipCount = (pageNumber - 1) * pageSize;

            // Lấy lịch sử đặt tour cho người dùng hiện tại
            var bookingHistoryQuery = (from b in _context.Bookings
                                       join t in _context.Tours on b.IdTour equals t.IdTour into tourGroup
                                       from t in tourGroup.DefaultIfEmpty() // Sử dụng join ngoài
                                       join h in _context.Hotels on b.IdHotel equals h.IdHotel into hotelGroup
                                       from h in hotelGroup.DefaultIfEmpty() // Sử dụng join ngoài
                                       join s in _context.BookingStatuses on b.IdStatus equals s.IdStatus into statusGroup
                                       from s in statusGroup.DefaultIfEmpty() // Sử dụng join ngoài
                                       where b.Id == currentUserId
                                       select new Booking
                                       {
                                           IdBooking = b.IdBooking,
                                           IdTourNavigation = t, // Có thể là null
                                           BookingTime = b.BookingTime,
                                           CheckInDate = b.CheckInDate,
                                           IdStatusNavigation = s, // Có thể là null
                                           IdHotelNavigation = h, // Có thể là null
                                       });

            // Lấy dữ liệu phân trang (skip và take)
            var bookingHistory = await bookingHistoryQuery
                .Skip(skipCount) // Bỏ qua số bản ghi đã qua
                .Take(pageSize)  // Lấy số bản ghi của trang hiện tại
                .ToListAsync();

            // Tính tổng số trang (total pages)
            var totalRecords = await bookingHistoryQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // Lưu thông tin phân trang vào ViewBag để hiển thị trong view
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;

            return View(bookingHistory);
        }

       public async Task<IActionResult> MyInvoices()
    {
        // Lấy ID người dùng hiện tại
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Lấy các hóa đơn mà người dùng đã thanh toán và chưa có hoàn tiền
        var invoices = await _context.Invoices
            .Where(i => i.PaymentInvoices.Any(pi => pi.IdPaymentNavigation.Id == userId &&
                                                     pi.IdPaymentNavigation.RefundAmount == null && 
                                                     pi.IdPaymentNavigation.RefundDate == null))
            .Select(i => new
            {
                i.IdInvoice,
                i.Description,
                i.TotalAmount,
                i.BillingDate,

                Payments = i.PaymentInvoices
                    .Select(pi => new
                    {
                        pi.PaidAmount,
                        pi.PaymentDate,
                        pi.Status,
                        pi.IdPaymentNavigation.RefundAmount,
                        pi.IdPaymentNavigation.RefundDate
                    }).ToList()
            })
            .ToListAsync();
            return View(invoices); 
    }
        // GET: History
        //public async Task<IActionResult> Index()
        //{
        //    var yourExistingDbContextName = _context.Bookings.Include(b => b.IdHotelNavigation).Include(b => b.IdNavigation).Include(b => b.IdStatusNavigation).Include(b => b.IdTourNavigation);
        //    return View(await yourExistingDbContextName.ToListAsync());
        //}

        // GET: History/Details/5
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

        // GET: History/Create
        public IActionResult Create()
        {
            ViewData["IdHotel"] = new SelectList(_context.Hotels, "IdHotel", "IdHotel");
            ViewData["Id"] = new SelectList(_context.AspNetUsers, "Id", "Id");
            ViewData["IdStatus"] = new SelectList(_context.BookingStatuses, "IdStatus", "IdStatus");
            ViewData["IdTour"] = new SelectList(_context.Tours, "IdTour", "IdTour");
            return View();
        }

        // POST: History/Create
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

        // GET: History/Edit/5
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

        // POST: History/Edit/5
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

        // GET: History/Delete/5
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

        // POST: History/Delete/5
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
