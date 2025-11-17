using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models;

namespace SchoolLibrary.Web.Areas.Student.Controllers
{
    [Area("Student")]
    public class BorrowController : Controller
    {
        private readonly LibraryDbContext _context;

        public BorrowController(LibraryDbContext context)
        {
            _context = context;
        }

        // Kiểm tra quyền Student
        private bool IsStudent()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Học sinh";
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserID");
        }

        // GET: Borrow History
        public async Task<IActionResult> Index()
        {
            if (!IsStudent())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var borrowRecords = await _context.BorrowRecords
                .Include(br => br.BookCopy)
                .ThenInclude(bc => bc.Book)
                .Where(br => br.UserID == userId)
                .OrderByDescending(br => br.BorrowDate)
                .ToListAsync();

            return View(borrowRecords);
        }

        // GET: Borrow Details
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsStudent())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var borrowRecord = await _context.BorrowRecords
                .Include(br => br.BookCopy)
                .ThenInclude(bc => bc.Book)
                .Include(br => br.User)
                .FirstOrDefaultAsync(br => br.BorrowID == id && br.UserID == userId);

            if (borrowRecord == null)
            {
                return NotFound();
            }

            return View(borrowRecord);
        }
    }
}

