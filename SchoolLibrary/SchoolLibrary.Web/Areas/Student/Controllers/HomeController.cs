using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models;
using System;

namespace SchoolLibrary.Web.Areas.Student.Controllers
{
    [Area("Student")]
    public class HomeController : Controller
    {
        private readonly LibraryDbContext _context;

        public HomeController(LibraryDbContext context)
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

        // GET: Dashboard
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

            // Query Reservations với try-catch để xử lý nếu bảng chưa tồn tại
            int activeReservations = 0;
            try
            {
                activeReservations = await _context.Reservations
                    .CountAsync(r => r.UserID == userId && r.Status == "Pending");
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không crash app
                // Có thể bảng chưa được tạo hoặc EF chưa reload schema
                activeReservations = 0;
            }

            var dashboard = new
            {
                TotalBorrowed = await _context.BorrowRecords
                    .CountAsync(br => br.UserID == userId && !br.IsReturned),
                TotalReturned = await _context.BorrowRecords
                    .CountAsync(br => br.UserID == userId && br.IsReturned),
                OverdueBooks = await _context.BorrowRecords
                    .Include(br => br.BookCopy)
                    .ThenInclude(bc => bc.Book)
                    .Where(br => br.UserID == userId && !br.IsReturned && br.DueDate < DateTime.Now)
                    .CountAsync(),
                ActiveReservations = activeReservations,
                RecentBorrows = await _context.BorrowRecords
                    .Include(br => br.BookCopy)
                    .ThenInclude(bc => bc.Book)
                    .Where(br => br.UserID == userId)
                    .OrderByDescending(br => br.BorrowDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(dashboard);
        }
    }
}

