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
    public class ToursManagerController : Controller
    {
        private readonly YourExistingDbContextName _context;
        private readonly AppDbContext _context1;
        public ToursManagerController(YourExistingDbContextName context, AppDbContext context1)
        {
            _context = context;
            _context1 = context1;
        }

        // GET: Host/ToursManager
        public async Task<IActionResult> Index(int page = 1, string searchTerm = "")
        {
            const int pageSize = 10; // Số bản ghi mỗi trang
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy danh sách tour có trạng thái "Chờ Duyệt" và IsDelete = "N"
            var query = _context.Tours
                .Include(t => t.IdHotelNavigation)
                .Include(t => t.IdTransNavigation)
                .Include(t => t.IdTypeNavigation)
                .Where(t => t.IsDelete == "N" && t.IdUser == userId); // Chỉ lấy tour có IsDelete = "N"

            // Thêm điều kiện tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(t => t.Name.Contains(searchTerm) || t.IdTypeNavigation.Name.Contains(searchTerm)); // Giả sử bạn tìm kiếm theo tên tour và tên loại tour
            }

            // Tính tổng số bản ghi
            var totalTours = await query.CountAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling(totalTours / (double)pageSize);

            // Lấy các bản ghi cho trang hiện tại
            var tours = await query
                .OrderBy(t => t.IdTour) // Thay đổi theo thuộc tính bạn muốn sắp xếp
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Gửi thông tin đến view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize; // Gửi pageSize đến view
            ViewBag.SearchTerm = searchTerm; // Gửi từ khóa tìm kiếm đến view

            return View(tours);
        }

        // GET: Host/ToursManager/Details/5
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
            var images = await _context1.Images.Where(i => i.IdTour == tour.IdTour).ToListAsync();
            tour.images = images;
            if (tour == null)
            {
                return NotFound();
            }

            return View(tour);
        }

        // GET: Host/ToursManager/Create
        public IActionResult Create()
        {
            // Tạo danh sách chọn cho các thuộc tính IdHotel, IdTrans, và IdType
            ViewData["IdHotel"] = new SelectList(_context.Hotels, "IdHotel", "Name");
            ViewData["IdTrans"] = new SelectList(_context.Transportations, "IdTrans", "Name");
            ViewData["IdType"] = new SelectList(_context.TypeOfTours, "IdType", "Name");
            return View();
        }

        // POST: Host/ToursManager/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,StartDate,EndDate,MaxQuantity,Price,IsDelete,IdType,IdTrans,IdHotel,ApprovalStatus")] Tour tour, IFormFileCollection imageFiles, IFormFile avatarFile)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var maxId = await _context.Tours.MaxAsync(p => p.IdTour);
                tour.IdTour = maxId + 1;
                tour.IsDelete = "N";
                tour.ApprovalStatus = "Chờ Duyệt";
                tour.IdUser = userId;

                // Nếu có ảnh đại diện
                if (avatarFile != null && avatarFile.Length > 0)
                {
                    var avatarFileName = Path.GetFileName(avatarFile.FileName);
                    var avatarFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", avatarFileName);

                    using (var stream = new FileStream(avatarFilePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(stream);
                    }

                    tour.Image = avatarFileName; // Lưu tên file vào trường Image của tour
                }

                _context.Add(tour);
                await _context.SaveChangesAsync();

                // Xử lý upload nhiều hình ảnh cho tour
                try
                {
                    if (imageFiles != null && imageFiles.Count > 0)
                    {
                        foreach (var imageFile in imageFiles)
                        {
                            if (imageFile.Length > 0)
                            {
                                var fileName = Path.GetFileName(imageFile.FileName);
                                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await imageFile.CopyToAsync(stream);
                                }

                                var image = new Image
                                {
                                    ImageUrl = fileName,
                                    IdTour = tour.IdTour,
                                    CreatedAt = DateTime.Now
                                };

                                _context1.Images.Add(image);
                            }
                        }

                        await _context1.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Thêm tour và hình ảnh thành công!";
                    }
                    else
                    {
                        TempData["WarningMessage"] = "Không có hình ảnh nào được tải lên.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu hình ảnh: " + ex.Message;
                }
                return RedirectToAction(nameof(Index)); // Điều hướng về trang Index
            }

            ViewData["IdHotel"] = new SelectList(_context.Hotels, "IdHotel", "Name", tour.IdHotel);
            ViewData["IdTrans"] = new SelectList(_context.Transportations, "IdTrans", "Name", tour.IdTrans);
            ViewData["IdType"] = new SelectList(_context.TypeOfTours, "IdType", "Name", tour.IdType);

            return View(tour);
        }



        public IActionResult CustomerRegistrationStatistics()
        {
            // Lấy Id người dùng hiện tại (người tổ chức)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy dữ liệu thống kê theo tour
            var tourStats = GetRegistrationStatsByTour(userId);

            // Lấy dữ liệu thống kê doanh thu theo tháng
            var bookingStats = GetBookingStatisticsByMonth(userId);

            // Tạo model cho view
            var model = new
            {
                TourStats = tourStats, // Thống kê theo tour
                BookingStats = bookingStats // Thống kê doanh thu
            };

            return View(model); // Trả về view với model
        }

        private List<object> GetRegistrationStatsByTour(string userId)
        {
            var statsByTour = _context.Bookings
                .Where(b => b.IdTourNavigation.IdUser == userId) // Lọc theo tour của người tổ chức
                .GroupBy(b => b.IdTour) // Nhóm theo tour
                .Select(g => new
                {
                    TourName = g.FirstOrDefault().IdTourNavigation.Name, // Lấy tên tour
                    TotalRegistrations = g.Count() // Tổng số lượt đăng ký cho tour
                })
                .OrderByDescending(g => g.TotalRegistrations) // Sắp xếp theo số lượt đăng ký
                .ToList();

            return statsByTour.Select(s => new { s.TourName, s.TotalRegistrations }).ToList<object>();
        }

        private List<object> GetBookingStatisticsByMonth(string userId)
        {
            var statsByMonth = _context.Bookings
                .Where(b => b.IdTourNavigation.IdUser == userId)
                .GroupBy(b => new { b.BookingTime.Value.Year, b.BookingTime.Value.Month })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    TotalRevenue = g.Sum(b => b.IdTourNavigation.Price), // Tổng doanh thu
                    TotalBookings = g.Count() // Số lượt đặt tour
                })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToList();

            return statsByMonth.Select(s => new { Month = $"{s.Year}-{s.Month}", s.TotalRevenue, s.TotalBookings }).ToList<object>();
        }






        // GET: Host/ToursManager/Edit/5
        // GET: Host/ToursManager/Edit/5
        public IActionResult Edit(long id)
        {
            var tour = _context.Tours.Find(id);
            if (tour == null)
            {
                return NotFound(); // Trả về 404 nếu tour không tồn tại
            }

            // Lấy danh sách hình ảnh từ context riêng
            var images = _context1.Images.Where(i => i.IdTour == tour.IdTour).ToList();

            ViewBag.Images = images; // Gán danh sách hình ảnh vào ViewBag

            ViewBag.TypeList = new SelectList(_context.TypeOfTours, "IdType", "Name", tour.IdType);
            ViewBag.TransportationList = new SelectList(_context.Transportations, "IdTrans", "Name", tour.IdTrans);
            ViewBag.HotelList = new SelectList(_context.Hotels, "IdHotel", "Name", tour.IdHotel);
            return View(tour);
        }

        // POST: Tour/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(long id, [Bind("IdTour,Name,Description,StartDate,EndDate,MaxQuantity,Price,IsDelete,IdType,IdTrans,IdHotel,ApprovalStatus")] Tour tour, List<Image> images)
        {
            if (id != tour.IdTour)
            {
                return NotFound(); // Kiểm tra xem ID có khớp không
            }

            if (ModelState.IsValid) // Kiểm tra tính hợp lệ của model
            {
                try
                {
                    // Tìm bản ghi hiện tại trong cơ sở dữ liệu
                    var existingTour = _context.Tours.Find(id);
                    if (existingTour == null)
                    {
                        return NotFound(); // Nếu tour không tồn tại
                    }

                    // Cập nhật các thuộc tính của tour
                    existingTour.Name = tour.Name;
                    existingTour.Description = tour.Description;
                    existingTour.StartDate = tour.StartDate;
                    existingTour.EndDate = tour.EndDate;
                    existingTour.MaxQuantity = tour.MaxQuantity;
                    existingTour.Price = tour.Price;
                    existingTour.IsDelete = tour.IsDelete;
                    existingTour.IdType = tour.IdType;
                    existingTour.IdTrans = tour.IdTrans;
                    existingTour.IdHotel = tour.IdHotel;
                    existingTour.ApprovalStatus = tour.ApprovalStatus;

                    // Cập nhật danh sách hình ảnh
                    foreach (var image in images)
                    {
                        var existingImage = _context1.Images.Find(image.IdTour); // Lấy hình ảnh theo ID
                        if (existingImage != null)
                        {
                            existingImage.ImageUrl = image.ImageUrl; // Cập nhật URL
                            existingImage.CreatedAt = existingImage.CreatedAt; // Giữ nguyên CreatedAt
                        }
                    }

                    // Lưu các thay đổi vào cơ sở dữ liệu
                    _context.SaveChanges();
                    _context1.SaveChanges(); // Đừng quên lưu thay đổi cho context hình ảnh

                    return RedirectToAction(nameof(Index)); // Chuyển hướng về action Index
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TourExists(tour.IdTour))
                    {
                        return NotFound(); // Nếu tour không tồn tại
                    }
                    else
                    {
                        throw; // Nếu có lỗi khác
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu dữ liệu: " + ex.Message);
                }
            }

            // Ghi lại lỗi ModelState để debug nếu có
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                Console.WriteLine(error.ErrorMessage);
            }

            ViewBag.TypeList = new SelectList(_context.TypeOfTours, "IdType", "Name", tour.IdType);
            ViewBag.TransportationList = new SelectList(_context.Transportations, "IdTrans", "Name", tour.IdTrans);
            ViewBag.HotelList = new SelectList(_context.Hotels, "IdHotel", "Name", tour.IdHotel);

            // Lấy lại danh sách hình ảnh
            var images1 = _context1.Images.Where(i => i.IdTour == tour.IdTour).ToList();
            ViewBag.Images = images;

            return View(tour); // Nếu không hợp lệ, hiển thị lại form với lỗi
        }



        // Phương thức riêng để lấy dữ liệu cho dropdown
        private async Task<IEnumerable<SelectListItem>> GetTourTypes()
        {
            return new SelectList(await _context.TypeOfTours.ToListAsync(), "IdType", "Name");
        }

        private async Task<IEnumerable<SelectListItem>> GetTransportations()
        {
            return new SelectList(await _context.Transportations.ToListAsync(), "IdTrans", "Name");
        }

        private async Task<IEnumerable<SelectListItem>> GetHotels()
        {
            return new SelectList(await _context.Hotels.ToListAsync(), "IdHotel", "Name");
        }

        // GET: Host/ToursManager/Delete/5
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

        // POST: Host/ToursManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour != null)
            {
                // Xóa các bản ghi trong PROMOTION liên quan đến tour
                var relatedPromotions = _context.Promotions.Where(p => p.IdTour == id).ToList();
                if (relatedPromotions.Any()) // Kiểm tra nếu có bản ghi liên quan
                {
                    _context.Promotions.RemoveRange(relatedPromotions);
                }

                // Xóa các bản ghi trong USERS_FAVORITE_TOUR liên quan đến tour
                var relatedFavoriteTours = _context.UsersFavoriteTours.Where(uft => uft.IdTour == id).ToList();
                if (relatedFavoriteTours.Any()) // Kiểm tra nếu có bản ghi liên quan
                {
                    _context.UsersFavoriteTours.RemoveRange(relatedFavoriteTours);
                }

                // Xóa các bản ghi trong TOUR_DETAILS liên quan đến tour
                var relatedTourDetails = _context.TourDetails.Where(td => td.IdTour == id).ToList();
                if (relatedTourDetails.Any()) // Kiểm tra nếu có bản ghi liên quan
                {
                    _context.TourDetails.RemoveRange(relatedTourDetails);
                }
                var relatedBookings = _context.Bookings.Where(b => b.IdTour == id).ToList();
                if (relatedBookings.Any())
                {
                    _context.Bookings.RemoveRange(relatedBookings);
                }
                // Xóa các báo cáo liên quan đến tour
                var relatedReports = _context.Reports.Where(r => r.IdTour == id).ToList();
                if (relatedReports.Any()) // Kiểm tra nếu có bản ghi liên quan
                {
                    _context.Reports.RemoveRange(relatedReports);
                }
                // Xóa các bình luận liên quan đến tour
                var relatedComments = _context.Comments.Where(c => c.IdTour == id).ToList();
                if (relatedComments.Any()) // Kiểm tra nếu có bình luận
                {
                    _context.Comments.RemoveRange(relatedComments);
                }

                // Xóa các hình ảnh liên quan đến tour
                var relatedImages = _context1.Images.Where(i => i.IdTour == id).ToList();
                foreach (var image in relatedImages)
                {
                    // Xóa file hình ảnh khỏi thư mục
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", image.ImageUrl);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }

                    // Xóa bản ghi hình ảnh khỏi cơ sở dữ liệu
                    _context1.Images.Remove(image);
                }

                // Xóa tour
                _context.Tours.Remove(tour);

                // Sử dụng giao dịch để đảm bảo tất cả các thay đổi đều được lưu
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Lưu thay đổi trong context hình ảnh
                        await _context1.SaveChangesAsync();

                        // Lưu thay đổi trong context tour và các bảng liên quan
                        await _context.SaveChangesAsync();

                        // Commit giao dịch
                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        // Nếu có lỗi, rollback giao dịch
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }




        private bool TourExists(long id)
        {
            return _context.Tours.Any(e => e.IdTour == id);
        }
    }
}
