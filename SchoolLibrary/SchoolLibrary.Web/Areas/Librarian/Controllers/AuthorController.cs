using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models;

namespace SchoolLibrary.Web.Areas.Librarian.Controllers
{
    [Area("Librarian")]
    public class AuthorController : Controller
    {
        private readonly LibraryDbContext _context;

        public AuthorController(LibraryDbContext context)
        {
            _context = context;
        }

        // Kiểm tra quyền Librarian
        private bool IsLibrarian()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Thủ thư" || role == "Librarian";
        }

        // GET: Author Index
        public async Task<IActionResult> Index()
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var authors = await _context.Authors
                .OrderBy(a => a.AuthorName)
                .ToListAsync();

            return View(authors);
        }

        // GET: Author Details
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

            var author = await _context.Authors
                .Include(a => a.BookAuthors)
                    .ThenInclude(ba => ba.Book)
                .FirstOrDefaultAsync(a => a.AuthorID == id);

            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // GET: Author Create
        public IActionResult Create()
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            return View();
        }

        // POST: Author Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Author author)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            if (ModelState.IsValid)
            {
                _context.Add(author);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm tác giả thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(author);
        }

        // GET: Author Edit
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

            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // POST: Author Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Author author)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            if (id != author.AuthorID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(author);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật tác giả thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuthorExists(author.AuthorID))
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

            return View(author);
        }

        // GET: Author Delete
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

            var author = await _context.Authors
                .Include(a => a.BookAuthors)
                    .ThenInclude(ba => ba.Book)
                .FirstOrDefaultAsync(a => a.AuthorID == id);

            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // POST: Author Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var author = await _context.Authors.FindAsync(id);
            if (author != null)
            {
                // Kiểm tra xem có sách nào đang sử dụng author này không
                var hasBooks = await _context.BookAuthors.AnyAsync(ba => ba.AuthorID == id);
                if (hasBooks)
                {
                    TempData["ErrorMessage"] = "Không thể xóa tác giả này vì đang có sách sử dụng!";
                    return RedirectToAction(nameof(Index));
                }

                _context.Authors.Remove(author);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa tác giả thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AuthorExists(int id)
        {
            return _context.Authors.Any(e => e.AuthorID == id);
        }
    }
}

