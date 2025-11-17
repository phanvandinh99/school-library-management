using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models;

namespace SchoolLibrary.Web.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class SuggestionController : Controller
    {
        private readonly LibraryDbContext _context;

        public SuggestionController(LibraryDbContext context)
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

        // GET: Suggestion List
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

            var suggestions = await _context.BookSuggestions
                .Where(bs => bs.UserID == userId)
                .OrderByDescending(bs => bs.SuggestionDate)
                .ToListAsync();

            return View(suggestions);
        }

        // GET: Create Suggestion
        public IActionResult Create()
        {
            if (!IsTeacher())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            return View();
        }

        // POST: Create Suggestion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookSuggestion suggestion)
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

            if (!ModelState.IsValid)
            {
                return View(suggestion);
            }

            suggestion.UserID = userId.Value;
            suggestion.SuggestionDate = DateTime.Now;
            suggestion.Status = "Pending";

            _context.BookSuggestions.Add(suggestion);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đề xuất mua sách đã được gửi thành công!";
            return RedirectToAction("Index");
        }

        // GET: Suggestion Details
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

            var suggestion = await _context.BookSuggestions
                .Include(bs => bs.User)
                .FirstOrDefaultAsync(bs => bs.SuggestionID == id && bs.UserID == userId);

            if (suggestion == null)
            {
                return NotFound();
            }

            return View(suggestion);
        }
    }
}

