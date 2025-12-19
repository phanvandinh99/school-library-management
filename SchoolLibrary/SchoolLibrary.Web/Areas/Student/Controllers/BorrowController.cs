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

        private async Task<int> GetDefaultBorrowDays()
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == "DefaultBorrowDays");
            return setting != null && int.TryParse(setting.SettingValue, out int days) ? days : 14;
        }

        private async Task<int> GetMaxBorrowBooks()
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == "MaxBorrowBooks");
            return setting != null && int.TryParse(setting.SettingValue, out int max) ? max : 5;
        }

        // POST: Borrow - Gửi yêu cầu mượn sách (cần thủ thư phê duyệt)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(int bookId)
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

            // Kiểm tra số sách đang mượn
            var currentBorrows = await _context.BorrowRecords
                .CountAsync(br => br.UserID == userId && !br.IsReturned);

            var maxBorrows = await GetMaxBorrowBooks();
            if (currentBorrows >= maxBorrows)
            {
                TempData["ErrorMessage"] = $"Bạn đã mượn tối đa {maxBorrows} cuốn sách!";
                return RedirectToAction("Details", "Book", new { id = bookId });
            }

            // Kiểm tra sách có tồn tại không
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                TempData["ErrorMessage"] = "Sách không tồn tại!";
                return RedirectToAction("Details", "Book", new { id = bookId });
            }

            // Kiểm tra đã có yêu cầu mượn chưa
            var existingRequest = await _context.Reservations
                .FirstOrDefaultAsync(r => r.UserID == userId && r.BookID == bookId && r.Status == "BorrowRequest");

            if (existingRequest != null)
            {
                TempData["ErrorMessage"] = "Bạn đã gửi yêu cầu mượn sách này rồi! Vui lòng chờ thủ thư phê duyệt.";
                return RedirectToAction("Details", "Book", new { id = bookId });
            }

            // Tạo yêu cầu mượn sách (Reservation với status "BorrowRequest")
            var reservation = new Reservation
            {
                UserID = userId.Value,
                BookID = bookId,
                ReservationDate = DateTime.Now,
                ExpiryDate = null, // Chưa có hạn khi chưa được phê duyệt
                Status = "BorrowRequest" // Phân biệt với "Pending" (đặt trước)
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã gửi yêu cầu mượn sách! Vui lòng chờ thủ thư phê duyệt.";
            return RedirectToAction(nameof(Index));
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

