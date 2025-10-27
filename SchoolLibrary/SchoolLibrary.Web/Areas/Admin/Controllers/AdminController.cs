using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models;
using SchoolLibrary.Web.Models.ViewModels;

namespace SchoolLibrary.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize] // Only logged in users
    public class AdminController : Controller
    {
        private readonly LibraryDbContext _context;

        public AdminController(LibraryDbContext context)
        {
            _context = context;
        }

        // Kiểm tra quyền Admin
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // GET: Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin())
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

        // GET: Index
        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            return RedirectToAction("Dashboard");
        }
    }
}

