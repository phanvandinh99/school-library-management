using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models.ViewModels;

namespace SchoolLibrary.Web.Areas.Librarian.Controllers
{
    [Area("Librarian")]
    public class HomeController : Controller
    {
        private readonly LibraryDbContext _context;

        public HomeController(LibraryDbContext context)
        {
            _context = context;
        }

        // Kiểm tra quyền Librarian
        private bool IsLibrarian()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Thủ thư" || role == "Librarian";
        }

        // GET: Dashboard
        public async Task<IActionResult> Index()
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            var dashboard = new DashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalBooks = await _context.Books.CountAsync(),
                TotalBorrowedBooks = await _context.BorrowRecords.CountAsync(br => !br.IsReturned),
                TotalCategories = await _context.Categories.CountAsync(),
                ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                InactiveUsers = await _context.Users.CountAsync(u => !u.IsActive),
                RecentBorrows = await _context.BorrowRecords
                    .Include(br => br.User)
                    .Include(br => br.BookCopy)
                    .ThenInclude(bc => bc.Book)
                    .OrderByDescending(br => br.BorrowDate)
                    .Take(10)
                    .ToListAsync(),
                RecentUsers = await _context.Users
                    .Include(u => u.Role)
                    .OrderByDescending(u => u.UserID)
                    .Take(5)
                    .ToListAsync()
            };

            return View(dashboard);
        }
    }
}

