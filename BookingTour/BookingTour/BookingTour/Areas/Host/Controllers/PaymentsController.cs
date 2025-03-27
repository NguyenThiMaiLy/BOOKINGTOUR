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
    public class PaymentsController : Controller
    {
        private readonly YourExistingDbContextName _context;

        public PaymentsController(YourExistingDbContextName context)
        {
            _context = context;
        }

        // GET: Host/Payments
        public async Task<IActionResult> Index()
        {
            var yourExistingDbContextName = _context.Payments.Include(p => p.IdNavigation);
            return View(await yourExistingDbContextName.ToListAsync());
        }
        public async Task<IActionResult> GetPaymentsForHost(int pageNumber = 1, int pageSize = 10, string searchTerm = "")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy danh sách các tour mà host quản lý
            var toursByHost = await _context.Tours
                .Where(t => t.IdUser == userId)
                .Select(t => t.IdTour)
                .ToListAsync();

            // Lấy tất cả booking của các tour mà host quản lý
            var bookings = await _context.Bookings
                .Where(b => toursByHost.Contains(b.IdTour))
                .ToListAsync();

             var userIds = bookings.Select(b => b.Id).Distinct().ToList();

    // Lấy thông tin người dùng (full name) từ bảng Users
    var users = await _context.AspNetUsers
        .Where(u => userIds.Contains(u.Id))  // Lọc theo Id người dùng từ bookings
        .ToListAsync();

    // Tạo một từ điển để dễ dàng tra cứu full name theo userId
    var userFullNames = users.ToDictionary(u => u.Id, u => u.fullname);

    // Truyền full name vào ViewBag
    ViewBag.UserFullNames = userFullNames;
            // Lấy tất cả hóa đơn (Invoice) từ các booking, thông qua InvoiceNavigation
            var invoices = bookings
                .Where(b => b.IdInvoice != null)
                .Select(b => b.IdInvoice.Value)
                .Distinct()
                .ToList();

            // Lấy các PaymentInvoice liên kết với các hóa đơn đã lọc
            var paymentInvoices = await _context.PaymentInvoices
                .Where(pi => invoices.Contains(pi.IdInvoice))
                .Include(pi => pi.IdInvoiceNavigation)
                .Include(pi => pi.IdPaymentNavigation)
                .ToListAsync();

            var paymentIds = paymentInvoices.Select(pi => pi.IdPayment).Distinct().ToList();

            // Tìm kiếm nếu có từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                paymentIds = paymentIds
                    .Where(paymentId => _context.Payments
                        .Where(p => p.IdPayment == paymentId)
                        .Any(p => p.IdNavigation.fullname.Contains(searchTerm) || p.IdPayment.ToString().Contains(searchTerm)) // Tìm kiếm theo tên người thanh toán hoặc mã thanh toán
                    )
                    .ToList();
            }


            // Lấy các payment và áp dụng phân trang
            var payments = await _context.Payments
                .Where(p => paymentIds.Contains(p.IdPayment))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
       
            // Gắn FullName vào từng payment
        
            // Tổng số trang
            int totalPayments = paymentIds.Count();
            int totalPages = (int)Math.Ceiling((double)totalPayments / pageSize);

            // Truyền dữ liệu phân trang và từ khóa tìm kiếm đến view
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = searchTerm; // Truyền từ khóa tìm kiếm đến view

            return View(payments);
        }





        // GET: Host/Payments/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.IdNavigation)
                .FirstOrDefaultAsync(m => m.IdPayment == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // GET: Host/Payments/Create
        public IActionResult Create()
        {
            ViewData["Id"] = new SelectList(_context.AspNetUsers, "Id", "Id");
            return View();
        }

        // POST: Host/Payments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPayment,Date,Method,Amount,Status,IsRefunded,RefundAmount,RefundDate,Id")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.AspNetUsers, "Id", "Id", payment.Id);
            return View(payment);
        }

        // GET: Host/Payments/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.AspNetUsers, "Id", "Id", payment.Id);
            return View(payment);
        }

        // POST: Host/Payments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("IdPayment,Date,Method,Amount,Status,IsRefunded,RefundAmount,RefundDate,Id")] Payment payment)
        {
            if (id != payment.IdPayment)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.IdPayment))
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
            ViewData["Id"] = new SelectList(_context.AspNetUsers, "Id", "Id", payment.Id);
            return View(payment);
        }

        // GET: Host/Payments/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.IdNavigation)
                .FirstOrDefaultAsync(m => m.IdPayment == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // POST: Host/Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentExists(long id)
        {
            return _context.Payments.Any(e => e.IdPayment == id);
        }
    }
}
