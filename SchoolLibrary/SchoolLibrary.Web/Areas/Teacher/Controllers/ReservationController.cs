using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models;
using System;

namespace SchoolLibrary.Web.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class ReservationController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly ILogger<ReservationController> _logger;

        public ReservationController(LibraryDbContext context, ILogger<ReservationController> logger)
        {
            _context = context;
            _logger = logger;
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

        // GET: Reservation List
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

            List<Reservation> reservations;
            try
            {
                reservations = await _context.Reservations
                    .Include(r => r.Book)
                    .ThenInclude(b => b.Category)
                    .Where(r => r.UserID == userId)
                    .OrderByDescending(r => r.ReservationDate)
                    .ToListAsync();
            }
            catch (SqlException ex) when (ex.Number == 208) // Invalid object name
            {
                // Log chi tiết lỗi
                var conn = _context.Database.GetDbConnection();
                var dbName = conn.Database ?? "Unknown";
                var server = conn.DataSource ?? "Unknown";
                
                _logger.LogError(ex, 
                    "SQL Error 208 - Invalid object name 'Reservations'. " +
                    "Database: {Database}, Server: {Server}, " +
                    "Error Number: {Number}, State: {State}, Class: {Class}, " +
                    "Message: {Message}",
                    dbName, server, ex.Number, ex.State, ex.Class, ex.Message);

                var errorDetails = $"Database: {dbName}, Server: {server}, " +
                                 $"Error: {ex.Message} (Number: {ex.Number}, State: {ex.State}, Class: {ex.Class})";
                
                TempData["ErrorMessage"] = $"✗ Lỗi! Bảng Reservations không tìm thấy. Chi tiết: {errorDetails}";
                reservations = new List<Reservation>();
            }
            catch (Exception ex)
            {
                var conn = _context.Database.GetDbConnection();
                var dbName = conn.Database ?? "Unknown";
                var server = conn.DataSource ?? "Unknown";
                
                _logger.LogError(ex, 
                    "Error loading reservations. Database: {Database}, Server: {Server}",
                    dbName, server);
                
                TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu: {ex.Message} (Database: {dbName}, Server: {server})";
                reservations = new List<Reservation>();
            }

            return View(reservations);
        }

        // POST: Create Reservation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int bookId)
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

            // Kiểm tra sách có tồn tại không
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                TempData["ErrorMessage"] = "Sách không tồn tại!";
                return RedirectToAction("Index", "Book");
            }

            // Kiểm tra đã đặt trước chưa
            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.UserID == userId && r.BookID == bookId && r.Status == "Pending");

            if (existingReservation != null)
            {
                TempData["ErrorMessage"] = "Bạn đã đặt trước cuốn sách này rồi!";
                return RedirectToAction("Details", "Book", new { id = bookId });
            }

            // Tạo reservation mới
            var reservation = new Reservation
            {
                UserID = userId.Value,
                BookID = bookId,
                ReservationDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddDays(7), // Hết hạn sau 7 ngày
                Status = "Pending"
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đặt trước sách thành công!";
            return RedirectToAction("Index");
        }

        // POST: Cancel Reservation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
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

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ReservationID == id && r.UserID == userId);

            if (reservation == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đặt trước!";
                return RedirectToAction("Index");
            }

            if (reservation.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Chỉ có thể hủy đặt trước đang chờ!";
                return RedirectToAction("Index");
            }

            reservation.Status = "Cancelled";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Hủy đặt trước thành công!";
            return RedirectToAction("Index");
        }
    }
}

