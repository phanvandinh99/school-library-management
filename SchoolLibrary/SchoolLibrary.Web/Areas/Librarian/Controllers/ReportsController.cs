using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Web.Data;

namespace SchoolLibrary.Web.Areas.Librarian.Controllers
{
    [Area("Librarian")]
    public class ReportsController : Controller
    {
        private readonly LibraryDbContext _context;

        public ReportsController(LibraryDbContext context)
        {
            _context = context;
        }

        private bool IsLibrarian()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Thủ thư" || role == "Librarian";
        }

        // GET: Index
        public IActionResult Index()
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        // GET: Weekly Report
        public async Task<IActionResult> WeeklyReport()
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            var weekStart = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);
            var weekEnd = weekStart.AddDays(6);

            var borrows = await _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.BookCopy)
                .ThenInclude(bc => bc.Book)
                .Where(br => br.BorrowDate >= weekStart && br.BorrowDate <= weekEnd)
                .ToListAsync();

            var returns = await _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.BookCopy)
                .ThenInclude(bc => bc.Book)
                .Where(br => br.ReturnDate.HasValue && br.ReturnDate.Value >= weekStart && br.ReturnDate.Value <= weekEnd)
                .ToListAsync();

            var mostBorrowedBooks = await _context.BorrowRecords
                .Include(br => br.BookCopy)
                .ThenInclude(bc => bc.Book)
                .Where(br => br.BorrowDate >= weekStart)
                .GroupBy(br => br.BookCopy.BookID)
                .Select(g => new
                {
                    BookTitle = g.First().BookCopy.Book.Title,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            ViewBag.WeekStart = weekStart.ToString("dd/MM/yyyy");
            ViewBag.WeekEnd = weekEnd.ToString("dd/MM/yyyy");
            ViewBag.Borrows = borrows;
            ViewBag.Returns = returns;
            ViewBag.MostBorrowedBooks = mostBorrowedBooks;

            return View();
        }

        // GET: Monthly Report
        public async Task<IActionResult> MonthlyReport(int? year, int? month)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            year ??= DateTime.Now.Year;
            month ??= DateTime.Now.Month;

            var monthStart = new DateTime(year.Value, month.Value, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var borrows = await _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.BookCopy)
                .ThenInclude(bc => bc.Book)
                .Where(br => br.BorrowDate >= monthStart && br.BorrowDate <= monthEnd)
                .ToListAsync();

            var returns = await _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.BookCopy)
                .ThenInclude(bc => bc.Book)
                .Where(br => br.ReturnDate.HasValue && br.ReturnDate.Value >= monthStart && br.ReturnDate.Value <= monthEnd)
                .ToListAsync();

            var userActivity = await _context.BorrowRecords
                .Include(br => br.User)
                .ThenInclude(u => u.Role)
                .Where(br => br.BorrowDate >= monthStart && br.BorrowDate <= monthEnd)
                .GroupBy(br => br.UserID)
                .Select(g => new
                {
                    UserName = g.First().User.FullName,
                    Role = g.First().User.Role.RoleName,
                    BorrowCount = g.Count()
                })
                .OrderByDescending(x => x.BorrowCount)
                .Take(20)
                .ToListAsync();

            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.MonthStart = monthStart.ToString("dd/MM/yyyy");
            ViewBag.MonthEnd = monthEnd.ToString("dd/MM/yyyy");
            ViewBag.Borrows = borrows;
            ViewBag.Returns = returns;
            ViewBag.UserActivity = userActivity;

            return View();
        }

        // GET: Statistics Summary
        public async Task<IActionResult> StatisticsSummary()
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            var totalUsers = await _context.Users.CountAsync();
            var totalBooks = await _context.Books.CountAsync();
            var totalBorrows = await _context.BorrowRecords.CountAsync();
            var totalActiveBorrows = await _context.BorrowRecords.CountAsync(br => !br.IsReturned);
            var totalCategories = await _context.Categories.CountAsync();
            var overdueBooks = await _context.BorrowRecords
                .CountAsync(br => !br.IsReturned && br.DueDate < DateTime.Now);

            var booksByCategory = await _context.Books
                .Include(b => b.Category)
                .GroupBy(b => b.CategoryID)
                .Select(g => new
                {
                    CategoryName = g.First().Category != null ? g.First().Category.CategoryName : "Không phân loại",
                    Count = g.Count()
                })
                .ToListAsync();

            var usersByRole = await _context.Users
                .Include(u => u.Role)
                .GroupBy(u => u.Role.RoleName)
                .Select(g => new
                {
                    RoleName = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalBooks = totalBooks;
            ViewBag.TotalBorrows = totalBorrows;
            ViewBag.TotalActiveBorrows = totalActiveBorrows;
            ViewBag.TotalCategories = totalCategories;
            ViewBag.OverdueBooks = overdueBooks;
            ViewBag.BooksByCategory = booksByCategory;
            ViewBag.UsersByRole = usersByRole;

            return View();
        }
    }
}

