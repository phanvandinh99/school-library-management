using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models;

namespace SchoolLibrary.Web.Areas.Librarian.Controllers
{
    [Area("Librarian")]
    public class PublisherController : Controller
    {
        private readonly LibraryDbContext _context;

        public PublisherController(LibraryDbContext context)
        {
            _context = context;
        }

        // Kiểm tra quyền Librarian
        private bool IsLibrarian()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Thủ thư" || role == "Librarian";
        }

        // GET: Publisher Index
        public async Task<IActionResult> Index()
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var publishers = await _context.Publishers
                .OrderBy(p => p.PublisherName)
                .ToListAsync();

            return View(publishers);
        }

        // GET: Publisher Details
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            if (id == null)
            {
                return NotFound();
            }

            var publisher = await _context.Publishers
                .Include(p => p.Books)
                .FirstOrDefaultAsync(p => p.PublisherID == id);

            if (publisher == null)
            {
                return NotFound();
            }

            return View(publisher);
        }

        // GET: Publisher Create
        public IActionResult Create()
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            return View();
        }

        // POST: Publisher Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Publisher publisher)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            if (ModelState.IsValid)
            {
                _context.Add(publisher);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm nhà xuất bản thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(publisher);
        }

        // GET: Publisher Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            if (id == null)
            {
                return NotFound();
            }

            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null)
            {
                return NotFound();
            }

            return View(publisher);
        }

        // POST: Publisher Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Publisher publisher)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            if (id != publisher.PublisherID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(publisher);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật nhà xuất bản thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PublisherExists(publisher.PublisherID))
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

            return View(publisher);
        }

        // GET: Publisher Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            if (id == null)
            {
                return NotFound();
            }

            var publisher = await _context.Publishers
                .Include(p => p.Books)
                .FirstOrDefaultAsync(p => p.PublisherID == id);

            if (publisher == null)
            {
                return NotFound();
            }

            return View(publisher);
        }

        // POST: Publisher Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher != null)
            {
                // Kiểm tra xem có sách nào đang sử dụng publisher này không
                var hasBooks = await _context.Books.AnyAsync(b => b.PublisherID == id);
                if (hasBooks)
                {
                    TempData["ErrorMessage"] = "Không thể xóa nhà xuất bản này vì đang có sách sử dụng!";
                    return RedirectToAction(nameof(Index));
                }

                _context.Publishers.Remove(publisher);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa nhà xuất bản thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PublisherExists(int id)
        {
            return _context.Publishers.Any(e => e.PublisherID == id);
        }
    }
}

