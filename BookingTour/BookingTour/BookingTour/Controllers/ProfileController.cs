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
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;

namespace BookingTour.Controllers
{

    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly YourExistingDbContextName _context;
        private readonly IWebHostEnvironment _environment;
        public ProfileController(YourExistingDbContextName context, IWebHostEnvironment environment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
        }

        // GET: Profile
        public async Task<IActionResult> Index()
        {
            return View(await _context.AspNetUsers.ToListAsync());
        }
        public IActionResult voucher()
        {
            // Lấy tất cả các khuyến mãi từ cơ sở dữ liệu
            var promotions = _context.Promotions.ToList();

            // Truyền danh sách khuyến mãi vào ViewBag
            ViewBag.Promotions = promotions;

            return View();
        }

        // GET: Profile/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aspNetUser = await _context.AspNetUsers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aspNetUser == null)
            {
                return NotFound();
            }

            return View(aspNetUser);
        }

        // GET: Profile/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Profile/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] AspNetUser aspNetUser)
        {
            if (ModelState.IsValid)
            {
                _context.Add(aspNetUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(aspNetUser);
        }

        // GET: Profile/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aspNetUser = await _context.AspNetUsers.FindAsync(id);
            if (aspNetUser == null)
            {
                return NotFound();
            }
            return View(aspNetUser);
        }

        // POST: Profile/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] AspNetUser aspNetUser)
        {
            if (id != aspNetUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aspNetUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AspNetUserExists(aspNetUser.Id))
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
            return View(aspNetUser);
        }

        // GET: Profile/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aspNetUser = await _context.AspNetUsers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aspNetUser == null)
            {
                return NotFound();
            }

            return View(aspNetUser);
        }

        // POST: Profile/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var aspNetUser = await _context.AspNetUsers.FindAsync(id);
            if (aspNetUser != null)
            {
                _context.AspNetUsers.Remove(aspNetUser);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AspNetUserExists(string id)
        {
            return _context.AspNetUsers.Any(e => e.Id == id);
        }
        public async Task<IActionResult> ViewProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy Id người dùng hiện tại
            var user = await _context.AspNetUsers.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Lấy danh sách khuyến mãi
            var promotions = await _context.Promotions.ToListAsync();

            // Lấy danh sách các tour yêu thích dựa trên IdTour từ bảng UsersFavoriteTour
            var favoriteTourIds = await _context.UsersFavoriteTours
                .Where(f => f.Id == userId) // Lọc theo id người dùng
                .Select(f => f.IdTour) // Lấy các IdTour mà người dùng yêu thích
                .ToListAsync();

            // Lấy thông tin chi tiết của các tour yêu thích từ bảng Tour
            var favoriteTours = await _context.Tours
                .Where(t => favoriteTourIds.Contains(t.IdTour)) // Lọc các tour có IdTour trùng với danh sách yêu thích
                .ToListAsync();

            var model = new UserProfileViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                fullname = user.fullname,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                Promotions = promotions,
                FavoriteTours = favoriteTours // Gửi danh sách các tour yêu thích
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(UserProfileViewModel model, IFormFile AvatarFile)
        {


            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại." });
            }

            // Cập nhật thông tin người dùng
            user.UserName = model.UserName;
            user.fullname = model.fullname;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.Email = model.Email;

            // Lưu avatar nếu có
            if (AvatarFile != null)
            {
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(AvatarFile.FileName);
                var avatarPath = Path.Combine(_environment.WebRootPath, "images", uniqueFileName);
                using (var stream = new FileStream(avatarPath, FileMode.Create))
                {
                    await AvatarFile.CopyToAsync(stream);
                }
                user.AvatarUrl = $"/images/{uniqueFileName}";
            }

            // Cập nhật vào cơ sở dữ liệu
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = "Cập nhật thành công!" }); // Trả về thông báo thành công
            }
            else
            {
                // Thêm thông báo lỗi vào ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật thông tin." });
            }
        }

    }
}
    