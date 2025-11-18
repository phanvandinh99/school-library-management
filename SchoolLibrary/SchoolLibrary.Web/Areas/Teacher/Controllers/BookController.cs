using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models;

namespace SchoolLibrary.Web.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class BookController : Controller
    {
        private readonly LibraryDbContext _context;

        public BookController(LibraryDbContext context)
        {
            _context = context;
        }

        // Kiểm tra quyền Teacher
        private bool IsTeacher()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Giáo viên";
        }

        // GET: Book Search
        public async Task<IActionResult> Index(string? searchTerm, int? categoryId)
        {
            if (!IsTeacher())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var query = _context.Books
                .Include(b => b.Category)
                .Include(b => b.BookCopies)
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b => 
                    b.Title.Contains(searchTerm) ||
                    (b.Author != null && b.Author.Contains(searchTerm)) ||
                    (b.ISBN != null && b.ISBN.Contains(searchTerm)));
            }

            // Lọc theo thể loại
            if (categoryId.HasValue)
            {
                query = query.Where(b => b.CategoryID == categoryId);
            }

            var books = await query
                .Select(b => new
                {
                    Book = b,
                    AvailableCopies = b.BookCopies.Count(bc => bc.Status == "Available"),
                    TotalCopies = b.BookCopies.Count
                })
                .ToListAsync();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CategoryId = categoryId;

            return View(books);
        }

        // GET: Book Details
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsTeacher())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
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

            var availableCopies = book.BookCopies.Count(bc => bc.Status == "Available");
            var userId = HttpContext.Session.GetInt32("UserID");

            // Kiểm tra xem đã đặt trước chưa
            bool hasReservation = false;
            if (userId.HasValue)
            {
                try
                {
                    hasReservation = await _context.Reservations
                        .AnyAsync(r => r.UserID == userId && r.BookID == id && r.Status == "Pending");
                }
                catch (SqlException ex) when (ex.Number == 208) // Invalid object name
                {
                    // Bảng chưa tồn tại, không có reservation
                    hasReservation = false;
                }
                catch
                {
                    hasReservation = false;
                }
            }

            ViewBag.AvailableCopies = availableCopies;
            ViewBag.HasReservation = hasReservation;

            return View(book);
        }
    }
}

