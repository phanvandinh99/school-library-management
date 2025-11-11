using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models;

namespace SchoolLibrary.Web.Areas.Librarian.Controllers
{
    [Area("Librarian")]
    public class CategoryController : Controller
    {
        private readonly LibraryDbContext _context;

        public CategoryController(LibraryDbContext context)
        {
            _context = context;
        }

        private bool IsLibrarian()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Thủ thư" || role == "Librarian";
        }

        // GET: Index
        public async Task<IActionResult> Index()
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            var categories = await _context.Categories
                .Include(c => c.Books)
                .ToListAsync();

            return View(categories);
        }

        // GET: Create
        public IActionResult Create()
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra trùng tên
                var exists = await _context.Categories
                    .AnyAsync(c => c.CategoryName == category.CategoryName);

                if (exists)
                {
                    ModelState.AddModelError("CategoryName", "Danh mục này đã tồn tại!");
                    return View(category);
                }

                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id != category.CategoryID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra trùng tên (trừ chính nó)
                var exists = await _context.Categories
                    .AnyAsync(c => c.CategoryName == category.CategoryName && c.CategoryID != id);

                if (exists)
                {
                    ModelState.AddModelError("CategoryName", "Danh mục này đã tồn tại!");
                    return View(category);
                }

                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryID))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            var category = await _context.Categories
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category != null)
            {
                // Kiểm tra xem có sách nào thuộc danh mục này không
                if (category.Books.Any())
                {
                    TempData["ErrorMessage"] = "Không thể xóa danh mục vì có sách thuộc danh mục này!";
                    return RedirectToAction(nameof(Index));
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa danh mục thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryID == id);
        }
    }
}

