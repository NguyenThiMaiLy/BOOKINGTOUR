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
using Microsoft.IdentityModel.Tokens;
using X.PagedList.Mvc.Core;
using X.PagedList;
namespace BookingTour.Controllers
{
    public class ToursController : Controller
    {
        private readonly YourExistingDbContextName _context;
        private readonly AppDbContext _context1;
        public ToursController(YourExistingDbContextName context, AppDbContext context1)
        {
            _context = context;
            _context1 = context1;
        }

        // GET: Tours
        public async Task<IActionResult> Index(int? page)
        {
            var toursQuery = _context.Tours
                .Include(t => t.IdHotelNavigation)
                .Include(t => t.IdTransNavigation)
                .Include(t => t.IdTypeNavigation)
                .Where(t => t.ApprovalStatus == "Đã phê duyệt" && t.IsDelete == "N");

            // Phân trang
            int pageSize = 10; // Số lượng item trên mỗi trang
            int pageNumber = (page ?? 1); // Nếu page null thì mặc định là trang 1

            // Tính toán số lượng tour từ dữ liệu
            var totalTours = await toursQuery.CountAsync();

            // Lấy các tour trong phạm vi trang hiện tại
            var tours = await toursQuery
                .Skip((pageNumber - 1) * pageSize) // Bỏ qua số lượng bản ghi của các trang trước
                .Take(pageSize) // Lấy số lượng bản ghi của trang hiện tại
                .ToListAsync();

            // Lấy các tour phổ biến
            ViewBag.PopularTours = await _context.Tours
                .Include(t => t.IdHotelNavigation)
                .Include(t => t.IdTransNavigation)
                .Include(t => t.IdTypeNavigation)
                .Where(t => t.ApprovalStatus == "Đã phê duyệt" && t.IsDelete == "N")
                .OrderByDescending(t => t.Price)
                //.Take(4)
                .ToListAsync();

            // Lấy các tour được đặt nhiều nhất
            ViewBag.MostBookedTours = await _context.Bookings
                .Where(b => b.IdTourNavigation.ApprovalStatus == "Đã phê duyệt" && b.IdTourNavigation.IsDelete == "N")
                .GroupBy(b => b.IdTour)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new
                {
                    Tour = g.FirstOrDefault().IdTourNavigation,
                    BookingCount = g.Count()
                })
                .ToListAsync();

            // Thêm thông tin phân trang vào ViewBag
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalTours / pageSize); // Tính tổng số trang
            ViewBag.CurrentPage = pageNumber;
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < ViewBag.TotalPages;

            var viewModel = new SearchTourViewModel
            {
                Tours = tours, // Dữ liệu tours đã phân trang
                Destinations = await _context.Destinations.ToListAsync(),
                TourTypes = await _context.TypeOfTours.ToListAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> TourList(int? page, long? typeId, decimal? maxPrice, string hotelName, string transportationName, string typeName, string priceOrder)
        {
            // Lấy danh sách các tour đã được phê duyệt và chưa bị xóa
            var toursQuery = _context.Tours
                .Include(t => t.IdHotelNavigation)
                .Include(t => t.IdTransNavigation)
                .Include(t => t.IdTypeNavigation)
                .Where(t => t.ApprovalStatus == "Đã phê duyệt" && t.IsDelete == "N");

            // Lọc theo loại tour nếu typeId được cung cấp
            if (typeId.HasValue)
            {
                toursQuery = toursQuery.Where(t => t.IdType == typeId.Value);
            }

            // Lọc theo giá tối đa nếu maxPrice được cung cấp
            if (maxPrice.HasValue)
            {
                toursQuery = toursQuery.Where(t => t.Price <= maxPrice.Value);
            }

            // Lọc theo tên khách sạn nếu hotelName được cung cấp
            if (!string.IsNullOrEmpty(hotelName))
            {
                toursQuery = toursQuery.Where(t => t.IdHotelNavigation.Name.Contains(hotelName));
            }

            // Lọc theo tên phương tiện nếu transportationName được cung cấp
            if (!string.IsNullOrEmpty(transportationName))
            {
                toursQuery = toursQuery.Where(t => t.IdTransNavigation.Name.Contains(transportationName));
            }

            // Lọc theo tên loại tour nếu typeName được cung cấp
            if (!string.IsNullOrEmpty(typeName))
            {
                toursQuery = toursQuery.Where(t => t.IdTypeNavigation.Name.Contains(typeName));
            }
            if (priceOrder == "asc")
            {
                toursQuery = toursQuery.OrderBy(t => t.Price);
            }
            else if (priceOrder == "desc")
            {
                toursQuery = toursQuery.OrderByDescending(t => t.Price);
            }
            // Phân trang
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var totalTours = await toursQuery.CountAsync();
            var tours = await toursQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Thêm thông tin phân trang vào ViewBag
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalTours / pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < ViewBag.TotalPages;

            var viewModel = new SearchTourViewModel
            {
                Tours = tours,
                Destinations = await _context.Destinations.ToListAsync(),
                TourTypes = await _context.TypeOfTours.ToListAsync() // Lấy danh sách TypeOfTour để hiển thị
            };

            return View(viewModel);
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
         
                .Include(t => t.Comments)
                .Include(t => t.UsersFavoriteTours)
                      .Include(t => t.UsersFavoriteTours.Where(c => c.IdTour == id))
                //.Include(t => t.Reports)
                //.Include(t => t.Reports.Where(c => c.IdTour == id))
                .ThenInclude(c => c.IdNavigation)
                .Include(t => t.Comments.Where(c => c.IdTour == id))
                .FirstOrDefaultAsync(m => m.IdTour == id );

            if (tour == null)
            {
                return NotFound();
            }
            var averageRating = tour.Comments.Any() ? tour.Comments.Average(c => c.Rating) : 0;
         
            // Truyền giá trị vào ViewBag
            ViewBag.AvgRating = averageRating;
            var images = await _context1.Images.Where(i => i.IdTour == tour.IdTour).ToListAsync();
            tour.images = images;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comment = await _context.Comments
                .Include(t => t.IdTourNavigation)
                .Include(t => t.IdNavigation)
                .FirstOrDefaultAsync(m => m.IdTour == id);
              ViewBag.UserId = userId;
            var favorite = _context.UsersFavoriteTours.FirstOrDefault(f => f.IdTour == id && f.Id == userId);
            var isFavorited = favorite != null;
            ViewBag.IsFavorited = isFavorited;
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.IdInvoice == id); // Thay thế `TourId` bằng tên thuộc tính chính xác

            if (invoice == null)
            {
                // Nếu chưa có hóa đơn, bạn có thể tạo một hóa đơn mới ở đây nếu cần
                // Hoặc chỉ cần trả về thông báo rằng hóa đơn chưa được tạo
                ViewBag.InvoiceId = null; // Hoặc tạo hóa đơn mới
            }
            else
            {
                ViewBag.InvoiceId = invoice.IdInvoice; // Gán Id hóa đơn vào ViewBag
            }

            ViewBag.Tour = tour;
            return View(tour);
        }


        // GET: Tours/Create
        public IActionResult Create(long tourId)
        {
            var tour = _context.Tours.Find(tourId);
            var booking = _context.Bookings.Find(tourId);
            if (tour == null)
            {
                return NotFound();
            }

            ViewBag.Tour = tour;
            return View();
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(long tourId, int quantity, [Bind("IdBooking,CheckInDate,CheckOutDate,BookingTime,Id,IdHotel,IdTour,IdStatus")] Booking booking1, decimal totalPrice)
        {
            

            var tour = await _context.Tours.FindAsync(tourId);
            if (tour == null)
            {
                return NotFound();
            }

            // Lấy Id người dùng từ Claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(); // Nếu không có thông tin người dùng
            }
            var invoices = new Invoice { };
            invoices.IdInvoice = _context.Invoices.Max(p => p.IdInvoice) + 1;
            var invoice = await CreateInvoice(invoices.IdInvoice, totalPrice);
            // Tạo đối tượng Booking
        
            booking1.IdInvoice= invoice.IdInvoice;
            booking1.IdTour = tourId;
                booking1.CheckInDate = DateTime.Now;// Thay đổi theo yêu cầu của bạn
            booking1.CheckOutDate = DateTime.Now.AddDays(1); // Thay đổi theo yêu cầu của bạn
            booking1.IdHotel = tour.IdHotel; // Lấy IdHotel từ tour
            booking1.IdStatus = 4; // Giả sử trạng thái đầu tiên là "Chưa thanh toán"
            booking1.BookingTime = DateTime.Now; // Thời gian đặt tour
            booking1.Id = userId; // Gán Id người dùng vào booking
                
            booking1.IdBooking = _context.Bookings.Max(p => p.IdBooking) + 1;
          
            // Lưu booking vào cơ sở dữ liệu
            _context.Bookings.Add(booking1);
            await _context.SaveChangesAsync();

            return RedirectToAction("Detaillnvoice", "Tours", new { id = invoice.IdInvoice }); 
        }
        public async Task<IActionResult> CancelBooking(long id)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.IdBooking == id);

            if (booking == null)
            {
                return NotFound(); // Nếu không tìm thấy booking
            }

            // Kiểm tra trạng thái để chỉ cho phép hủy nếu trạng thái là "Confirmed"
            if (booking.IdStatus == 1 || booking.IdStatus == 4)
            {
                booking.IdStatus = 3; // Cập nhật trạng thái thành "Đã hủy" (giả sử 3 là hủy)

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Hủy tour thành công!";

                // Quay lại trang danh sách các tour
                return RedirectToAction("Index", "History");
            }
            else
            {
                // Thêm thông báo lỗi vào TempData nếu không thể hủy tour
                TempData["ErrorMessage"] = "Không thể hủy tour trong trạng thái này.";

                return RedirectToAction("Index", "History");
            }
        }
        public async Task<IActionResult> CancelInvoice(long id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.PaymentInvoices)
                .ThenInclude(pi => pi.IdPaymentNavigation)
                .FirstOrDefaultAsync(i => i.IdInvoice == id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice); // View để xác nhận hủy và hiển thị chính sách hoàn tiền
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmCancelInvoice(long id, decimal refundAmount)
        {
            var invoice = await _context.Invoices
                .Include(i => i.PaymentInvoices)
                .ThenInclude(pi => pi.IdPaymentNavigation)
                .FirstOrDefaultAsync(i => i.IdInvoice == id);

            if (invoice == null)
            {
                return NotFound();
            }
            // Lấy các booking liên quan đến hóa đơn này (Giả sử có mối quan hệ giữa invoice và booking)
            var bookingsToDelete = _context.Bookings
                .Where(b => b.IdInvoice == id) // Điều kiện này phụ thuộc vào mối quan hệ của bạn với bảng Bookings
                .ToList();

            if (bookingsToDelete.Any())
            {
                _context.Bookings.RemoveRange(bookingsToDelete);  // Xóa tất cả các booking liên quan
            }

            foreach (var paymentInvoice in invoice.PaymentInvoices)
            {
                paymentInvoice.IdPaymentNavigation.RefundAmount = refundAmount;
                paymentInvoice.IdPaymentNavigation.RefundDate = DateTime.Now;
                paymentInvoice.IdPaymentNavigation.Status = "Canceled";
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Hủy hóa đơn thành công và hoàn tiền đã được cập nhật.";
            return RedirectToAction("MyInvoices","History");
        }
        public async Task<IActionResult> ViewInvoice(long id)
        {
            // Tìm booking theo Id
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.IdBooking == id);

            if (booking == null)
            {
                return NotFound(); // Nếu booking không tồn tại
            }

            // Tìm hóa đơn theo IdInvoice của booking
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.IdInvoice == booking.IdInvoice);

            if (invoice == null)
            {
                return NotFound("Invoice not found for this booking"); // Nếu không tìm thấy hóa đơn
            }

            // Trả về View và truyền dữ liệu invoice
            return View(invoice);
        }
        public async Task<IActionResult> Search(string destinationAddress, DateTime? startDate, string tourType)
        {
            ViewBag.SelectedDestinationAddress = destinationAddress;
            ViewBag.SelectedStartDate = startDate;
            ViewBag.SelectedTourType = tourType;
            var viewModel = new SearchTourViewModel
            {
                Destinations = await _context.Destinations.ToListAsync(),
                TourTypes = await _context.TypeOfTours.ToListAsync(),
                Tourd   = await _context.Tours.ToListAsync()
               
            };

            // Lấy danh sách các tour dưới dạng IQueryable
            var toursQuery = _context.Tours
                .Include(t => t.TourDetails)
                    .ThenInclude(td => td.IdDesNavigation) // Bao gồm Destination
                .Include(t => t.IdTypeNavigation) // Bao gồm TypeOfTour
                .AsQueryable(); // Chuyển đổi sang IQueryable

            // Thực hiện các điều kiện tìm kiếm
            if (!string.IsNullOrEmpty(destinationAddress))
            {
                toursQuery = toursQuery.Where(t => t.TourDetails
                    .Any(td => td.IdDesNavigation.Address.Contains(destinationAddress)));
            }

            if (startDate.HasValue)
            {
                toursQuery = toursQuery.Where(t => t.StartDate == startDate);
            }

            if (!string.IsNullOrEmpty(tourType))
            {
                toursQuery = toursQuery.Where(t => t.IdTypeNavigation.Name == tourType);
            }

            // Chuyển đổi sang danh sách và thực hiện truy vấn
            var resultTours = await toursQuery.ToListAsync(); // Gọi ToListAsync trên IQueryable

            // Trả về view với model
            viewModel.Tours = resultTours; // Gán danh sách tour đã lọc vào viewModel
            return View("serch", viewModel); // Trả về view với SearchTourViewModel
        }

        public IActionResult Detaillnvoice(int id, int? selectedPromotionId)
        {
            var invoice = _context.Invoices
                .Include(i => i.PaymentInvoices)
                .FirstOrDefault(i => i.IdInvoice == id);

            if (invoice == null)
            {
                return NotFound();
            }

            var booking = _context.Bookings
                .FirstOrDefault(b => b.IdInvoice == id);

            if (booking == null)
            {
                return NotFound();
            }

            long idTour = booking.IdTour;
            var promotions = _context.Promotions
                .Where(p => p.IdTour == idTour && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now && p.Status == "Active")
                .ToList();

            ViewBag.Promotions = promotions;

            // Áp dụng mã giảm giá được chọn nếu có
            if (selectedPromotionId.HasValue)
            {
                var selectedPromotion = promotions.FirstOrDefault(p => p.IdPmt == selectedPromotionId.Value);
                if (selectedPromotion != null)
                {
                    decimal totalAmount = invoice.TotalAmount ?? 0;
                    decimal discountPercent = selectedPromotion.DiscountAmount ?? 0;
                    decimal totalAfterDiscount = totalAmount - (totalAmount * discountPercent / 100);
                    invoice.TotalAmount = totalAfterDiscount;

                    // Lưu lại thay đổi vào cơ sở dữ liệu
                    _context.SaveChanges();
                    ViewBag.TotalAfterDiscount = totalAfterDiscount;
                    ViewBag.DiscountAmount = discountPercent;
                    ViewBag.SelectedPromotionId = selectedPromotionId.Value;
                }
            }

            return View(invoice);
        }


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
     
        public async Task<Invoice> CreateInvoice( long id,decimal totalAmount)
        {
          

          
            var invoice = new Invoice
            {
                IdInvoice = id,
                Description = "Thanh toán cho tour đã đặt",
                TotalAmount = totalAmount,
                BillingDate = DateTime.Now,
                CreatedAt = DateTime.Now
            };
           
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return invoice;
        }
        public decimal CalculatePaymentAmount(decimal totalAmount, bool isFullPayment)
        {
            return isFullPayment ? totalAmount : totalAmount * 0.5m;
        }
        public async Task<Payment> CreatePayment( decimal amount, string method, bool isFullPayment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var payment = new Payment
            {
                Date = DateTime.Now,
                Method = method, // "Ngân hàng" hoặc "MoMo"
                Amount = amount,
                Status = "Đang xử lý",
                IsRefunded = "N",
                Id = userId // Lưu userId của người thực hiện thanh toán
            };
            payment.IdPayment = await _context.Payments.Select(p => p.IdPayment).DefaultIfEmpty(0).MaxAsync() + 1;
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return payment;
        }
        public async Task<PaymentInvoice> LinkPaymentToInvoice(long paymentId, long invoiceId, decimal paidAmount)
        {
            var paymentInvoice = new PaymentInvoice
            {
                IdPayment = paymentId,
                IdInvoice = invoiceId,
                PaidAmount = paidAmount,
                PaymentDate = DateTime.Now,
                Status = "Hoàn thành"
            };

            _context.PaymentInvoices.Add(paymentInvoice);
            await _context.SaveChangesAsync();

            return paymentInvoice;
        }
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(long invoiceId, string method, bool isFullPayment)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null)
            {
                return NotFound("Invoice not found");
            }

            // Kiểm tra và lấy giá trị TotalAmount
            if (!invoice.TotalAmount.HasValue)
            {
                return BadRequest("Total amount is not set.");
            }

            decimal amountToPay = CalculatePaymentAmount(invoice.TotalAmount.Value, isFullPayment);
            var payment = await CreatePayment(amountToPay, method, isFullPayment);
            await LinkPaymentToInvoice(payment.IdPayment, invoice.IdInvoice, amountToPay);

            return RedirectToAction("PaymentConfirmation",invoiceId);
        }
        [HttpGet]
        public async Task<IActionResult> PaymentConfirmation(long invoiceId)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId); // Tìm hóa đơn theo ID
            if (invoice == null)
            {
                return NotFound(); // Trả về lỗi 404 nếu không tìm thấy hóa đơn
            }

            // Tạo mô hình để truyền đến view
            var model = new PaymentConfirmationViewModel
            {
                IdInvoice = invoice.IdInvoice,
                TotalAmount = invoice.TotalAmount,
                BillingDate = invoice.BillingDate ?? DateTime.Now, // Sử dụng DateTime.Now nếu BillingDate là null
                Description = invoice.Description,
                CreatedAt = invoice.CreatedAt ?? DateTime.Now
            };

            return View(model); // Trả về view với mô hình
        }
        // POST: Tours/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("IdTour,Name,Description,Image,StartDate,EndDate,MaxQuantity,Price,IsDelete,IdType,IdTrans,IdHotel,ApprovalStatus")] Tour tour)
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
    }
}
