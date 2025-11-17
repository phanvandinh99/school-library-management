using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models;

namespace SchoolLibrary.Web.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class BorrowController : Controller
    {
        private readonly LibraryDbContext _context;

        public BorrowController(LibraryDbContext context)
        {
            _context = context;
        }

        // Kiểm tra quyền Teacher
        private bool IsTeacher()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Giáo viên";
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserID");
        }

        // GET: Borrow History
        public async Task<IActionResult> Index()
        {
            if (!IsTeacher())
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
            if (!IsTeacher())
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

