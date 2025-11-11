using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models;

namespace SchoolLibrary.Web.Areas.Librarian.Controllers
{
    [Area("Librarian")]
    public class BookController : Controller
    {
        private readonly LibraryDbContext _context;

        public BookController(LibraryDbContext context)
        {
            _context = context;
        }

        private bool IsLibrarian()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Thủ thư" || role == "Librarian";
        }

        // GET: Index
        public async Task<IActionResult> Index(string? searchTerm)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            var books = _context.Books.Include(b => b.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                books = books.Where(b =>
                    b.Title.Contains(searchTerm) ||
                    b.Author.Contains(searchTerm) ||
                    b.ISBN.Contains(searchTerm));
            }

            var bookList = await books
                .Select(b => new
                {
                    Book = b,
                    AvailableCopies = b.BookCopies.Count(bc => bc.Status == "Available"),
                    TotalCopies = b.BookCopies.Count
                })
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            return View(bookList.Select(x => x.Book).ToList());
        }

        // GET: Details
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.BookCopies)
                .FirstOrDefaultAsync(b => b.BookID == id);

            if (book == null)
            {
                return NotFound();
            }

            ViewBag.AvailableCopies = book.BookCopies.Count(bc => bc.Status == "Available");
            ViewBag.BorrowedCopies = book.BookCopies.Count(bc => bc.Status == "Borrowed");

            return View(book);
        }

        // GET: Create
        public async Task<IActionResult> Create()
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryID", "CategoryName");
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book, int numberOfCopies = 1)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();

                // Tạo các bản sao sách
                for (int i = 1; i <= numberOfCopies; i++)
                {
                    var copy = new BookCopy
                    {
                        BookID = book.BookID,
                        CopyCode = $"{book.ISBN ?? book.BookID.ToString()}-{i:D3}",
                        Status = "Available"
                    };
                    _context.BookCopies.Add(copy);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm sách thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryID", "CategoryName", book.CategoryID);
            return View(book);
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

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryID", "CategoryName", book.CategoryID);
            return View(book);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id != book.BookID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật sách thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.BookID))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryID", "CategoryName", book.CategoryID);
            return View(book);
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

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.BookID == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
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

            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                // Kiểm tra xem có sách đang được mượn không
                var hasBorrowedCopies = await _context.BookCopies
                    .AnyAsync(bc => bc.BookID == id && bc.Status == "Borrowed");

                if (hasBorrowedCopies)
                {
                    TempData["ErrorMessage"] = "Không thể xóa sách vì có bản sao đang được mượn!";
                    return RedirectToAction(nameof(Index));
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa sách thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookID == id);
        }
    }
}

