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

namespace BookingTour.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BlogsController : Controller
    {
        private readonly YourExistingDbContextName _context;

        public BlogsController(YourExistingDbContextName context)
        {
            _context = context;
        }

        // GET: Admin/Blogs
        public async Task<IActionResult> Index()
        {
            return View(await _context.Blog.ToListAsync());
        }

        // GET: Admin/Blogs/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blog
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // GET: Admin/Blogs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Blogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog blog, IFormFile ThumbnailUrl, IFormFile ImageUrl)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Xử lý ảnh thu nhỏ
            if (ThumbnailUrl != null)
                {
                    var thumbnailFileName = Path.GetFileName(ThumbnailUrl.FileName);
                    var thumbnailFilePath = Path.Combine("wwwroot/images", thumbnailFileName);
                    using (var stream = new FileStream(thumbnailFilePath, FileMode.Create))
                    {
                        await ThumbnailUrl.CopyToAsync(stream);
                    }
                    blog.ThumbnailUrl = thumbnailFileName;
                }

                // Xử lý ảnh chính
                if (ImageUrl != null)
                {
                    var imageFileName = Path.GetFileName(ImageUrl.FileName);
                    var imageFilePath = Path.Combine("wwwroot/images", imageFileName);
                    using (var stream = new FileStream(imageFilePath, FileMode.Create))
                    {
                        await ImageUrl.CopyToAsync(stream);
                    }
                    blog.ImageUrl = imageFileName;
                }
            blog.Id = userId.ToString();
            blog.CreatedAt = DateTime.Now;
                _context.Add(blog);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));  // Điều hướng đến danh sách blog
            

            // Nếu model không hợp lệ, trở lại trang create
        }


        // GET: Admin/Blogs/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blog.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }
            return View(blog);
        }

        // POST: Admin/Blogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IdBlog,Title,Content,Id,CreatedAt,UpdatedAt,Status,ThumbnailUrl,ImageUrl,Tags")] Blog blog)
        {
            if (id != blog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(blog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(blog.Id))
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
            return View(blog);
        }

        // GET: Admin/Blogs/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blog
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // POST: Admin/Blogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var blog = await _context.Blog.FindAsync(id);
            if (blog != null)
            {
                _context.Blog.Remove(blog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BlogExists(string id)
        {
            return _context.Blog.Any(e => e.Id == id);
        }
    }
}
