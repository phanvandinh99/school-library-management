using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models;

namespace SchoolLibrary.Web.Areas.Librarian.Controllers
{
    [Area("Librarian")]
    public class BorrowController : Controller
    {
        private readonly LibraryDbContext _context;

        public BorrowController(LibraryDbContext context)
        {
            _context = context;
        }

        private bool IsLibrarian()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Thủ thư" || role == "Librarian";
        }

        private async Task<int> GetDefaultBorrowDays()
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == "DefaultBorrowDays");
            return setting != null && int.TryParse(setting.SettingValue, out int days) ? days : 14;
        }

        private async Task<int> GetMaxRenewDays()
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == "MaxRenewDays");
            return setting != null && int.TryParse(setting.SettingValue, out int days) ? days : 7;
        }

        private async Task<decimal> GetFinePerDay()
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == "FinePerDay");
            return setting != null && decimal.TryParse(setting.SettingValue, out decimal fine) ? fine : 5000;
        }

        private async Task<int> GetMaxBorrowBooks()
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == "MaxBorrowBooks");
            return setting != null && int.TryParse(setting.SettingValue, out int max) ? max : 5;
        }

        // GET: Index - Danh sách mượn/trả
        public async Task<IActionResult> Index(string? status)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            var borrows = _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.BookCopy)
                .ThenInclude(bc => bc.Book)
                .AsQueryable();

            if (status == "active")
            {
                borrows = borrows.Where(br => !br.IsReturned);
            }
            else if (status == "returned")
            {
                borrows = borrows.Where(br => br.IsReturned);
            }
            else if (status == "overdue")
            {
                borrows = borrows.Where(br => !br.IsReturned && br.DueDate < DateTime.Now);
            }

            var borrowList = await borrows
                .OrderByDescending(br => br.BorrowDate)
                .ToListAsync();

            ViewBag.Status = status;
            return View(borrowList);
        }

        // GET: Borrow - Form mượn sách
        public async Task<IActionResult> Borrow()
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Users = new SelectList(
                await _context.Users
                    .Where(u => u.IsActive && (u.RoleID == 1 || u.RoleID == 2)) // Student or Teacher
                    .ToListAsync(),
                "UserID", "FullName");

            var availableBooks = await _context.BookCopies
                .Include(bc => bc.Book)
                .Where(bc => bc.Status == "Available")
                .Select(bc => new
                {
                    CopyID = bc.CopyID,
                    Display = $"{bc.Book.Title} - {bc.CopyCode}"
                })
                .ToListAsync();

            ViewBag.AvailableBooks = new SelectList(
                availableBooks,
                "CopyID", "Display");

            return View();
        }

        // POST: Borrow
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(int userId, int copyId)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            // Kiểm tra số sách đang mượn
            var currentBorrows = await _context.BorrowRecords
                .CountAsync(br => br.UserID == userId && !br.IsReturned);

            var maxBorrows = await GetMaxBorrowBooks();
            if (currentBorrows >= maxBorrows)
            {
                TempData["ErrorMessage"] = $"Người dùng đã mượn tối đa {maxBorrows} cuốn sách!";
                return RedirectToAction(nameof(Borrow));
            }

            // Kiểm tra sách có sẵn không
            var bookCopy = await _context.BookCopies
                .Include(bc => bc.Book)
                .FirstOrDefaultAsync(bc => bc.CopyID == copyId);

            if (bookCopy == null || bookCopy.Status != "Available")
            {
                TempData["ErrorMessage"] = "Sách không có sẵn để mượn!";
                return RedirectToAction(nameof(Borrow));
            }

            var defaultDays = await GetDefaultBorrowDays();
            var borrowRecord = new BorrowRecord
            {
                UserID = userId,
                CopyID = copyId,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(defaultDays),
                IsReturned = false,
                FineAmount = 0
            };

            bookCopy.Status = "Borrowed";
            _context.BorrowRecords.Add(borrowRecord);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Mượn sách thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Return
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            var borrowRecord = await _context.BorrowRecords
                .Include(br => br.BookCopy)
                .FirstOrDefaultAsync(br => br.BorrowID == id);

            if (borrowRecord == null || borrowRecord.IsReturned)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bản ghi mượn sách!";
                return RedirectToAction(nameof(Index));
            }

            borrowRecord.IsReturned = true;
            borrowRecord.ReturnDate = DateTime.Now;
            borrowRecord.BookCopy.Status = "Available";

            // Tính phạt nếu trễ hạn
            if (borrowRecord.ReturnDate > borrowRecord.DueDate)
            {
                var daysLate = (borrowRecord.ReturnDate.Value - borrowRecord.DueDate).Days;
                var finePerDay = await GetFinePerDay();
                borrowRecord.FineAmount = daysLate * finePerDay;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Trả sách thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Renew
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renew(int id)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Login", "Auth");
            }

            var borrowRecord = await _context.BorrowRecords
                .FirstOrDefaultAsync(br => br.BorrowID == id);

            if (borrowRecord == null || borrowRecord.IsReturned)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bản ghi mượn sách!";
                return RedirectToAction(nameof(Index));
            }

            var maxRenewDays = await GetMaxRenewDays();
            borrowRecord.DueDate = borrowRecord.DueDate.AddDays(maxRenewDays);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Gia hạn thành công! Hạn mới: {borrowRecord.DueDate:dd/MM/yyyy}";
            return RedirectToAction(nameof(Index));
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

            var borrowRecord = await _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.BookCopy)
                .ThenInclude(bc => bc.Book)
                .FirstOrDefaultAsync(br => br.BorrowID == id);

            if (borrowRecord == null)
            {
                return NotFound();
            }

            return View(borrowRecord);
        }
    }
}

