using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models;
using SchoolLibrary.Web.Models.ViewModels;

namespace SchoolLibrary.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ActivityMonitorController : Controller
    {
        private readonly LibraryDbContext _context;

        public ActivityMonitorController(LibraryDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // GET: Index - Overview
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var today = DateTime.Now.Date;
            var weekAgo = today.AddDays(-7);
            var monthAgo = today.AddDays(-30);

            var recentBorrows = await _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.BookCopy)
                .ThenInclude(bc => bc.Book)
                .OrderByDescending(br => br.BorrowDate)
                .Take(50)
                .ToListAsync();

            var overdueBooks = await _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.BookCopy)
                .ThenInclude(bc => bc.Book)
                .Where(br => !br.IsReturned && br.DueDate < today)
                .OrderBy(br => br.DueDate)
                .ToListAsync();

            var statistics = new
            {
                TotalBorrowsToday = await _context.BorrowRecords.CountAsync(br => br.BorrowDate.Date == today),
                TotalBorrowsThisWeek = await _context.BorrowRecords.CountAsync(br => br.BorrowDate >= weekAgo),
                TotalBorrowsThisMonth = await _context.BorrowRecords.CountAsync(br => br.BorrowDate >= monthAgo),
                OverdueCount = overdueBooks.Count,
                ReturnedToday = await _context.BorrowRecords.CountAsync(br => br.ReturnDate.HasValue && br.ReturnDate.Value.Date == today)
            };

            ViewBag.RecentBorrows = recentBorrows;
            ViewBag.OverdueBooks = overdueBooks;
            ViewBag.Statistics = statistics;

            return View();
        }

        // GET: OverdueBooks
        public async Task<IActionResult> OverdueBooks()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var today = DateTime.Now.Date;
            var overdue = await _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.BookCopy)
                .ThenInclude(bc => bc.Book)
                .Where(br => !br.IsReturned && br.DueDate < today)
                .OrderBy(br => br.DueDate)
                .ToListAsync();

            return View(overdue);
        }

        // POST: MarkAsRead
        [HttpPost]
        public IActionResult MarkAsNotified(int borrowId)
        {
            // This can be expanded with a notification tracking table
            return Json(new { success = true, message = "Đã đánh dấu đã thông báo" });
        }
    }
}

