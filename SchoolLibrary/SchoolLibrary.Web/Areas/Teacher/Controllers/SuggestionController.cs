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
    public class SuggestionController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly ILogger<SuggestionController> _logger;

        public SuggestionController(LibraryDbContext context, ILogger<SuggestionController> logger)
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

            List<BookSuggestion> suggestions;
            try
            {
                suggestions = await _context.BookSuggestions
                    .Where(bs => bs.UserID == userId)
                    .OrderByDescending(bs => bs.SuggestionDate)
                    .ToListAsync();
            }
            catch (SqlException ex) when (ex.Number == 208) // Invalid object name
            {
                // Log chi tiết lỗi
                var conn = _context.Database.GetDbConnection();
                var dbName = conn.Database ?? "Unknown";
                var server = conn.DataSource ?? "Unknown";
                
                _logger.LogError(ex, 
                    "SQL Error 208 - Invalid object name 'BookSuggestions'. " +
                    "Database: {Database}, Server: {Server}, " +
                    "Error Number: {Number}, State: {State}, Class: {Class}, " +
                    "Message: {Message}",
                    dbName, server, ex.Number, ex.State, ex.Class, ex.Message);

                var errorDetails = $"Database: {dbName}, Server: {server}, " +
                                 $"Error: {ex.Message} (Number: {ex.Number}, State: {ex.State}, Class: {ex.Class})";
                
                TempData["ErrorMessage"] = $"✗ Lỗi! Bảng BookSuggestions không tìm thấy. Chi tiết: {errorDetails}";
                suggestions = new List<BookSuggestion>();
            }
            catch (Exception ex)
            {
                var conn = _context.Database.GetDbConnection();
                var dbName = conn.Database ?? "Unknown";
                var server = conn.DataSource ?? "Unknown";
                
                _logger.LogError(ex, 
                    "Error loading suggestions. Database: {Database}, Server: {Server}",
                    dbName, server);
                
                TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu: {ex.Message} (Database: {dbName}, Server: {server})";
                suggestions = new List<BookSuggestion>();
            }

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

            try
            {
                _context.BookSuggestions.Add(suggestion);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đề xuất mua sách đã được gửi thành công!";
            }
            catch (SqlException ex) when (ex.Number == 208) // Invalid object name
            {
                TempData["ErrorMessage"] = "Bảng BookSuggestions chưa được tạo trong database. Vui lòng chạy script SQL để tạo bảng.";
                return View(suggestion);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi lưu dữ liệu: {ex.Message}";
                return View(suggestion);
            }

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

            BookSuggestion? suggestion;
            try
            {
                suggestion = await _context.BookSuggestions
                    .Include(bs => bs.User)
                    .FirstOrDefaultAsync(bs => bs.SuggestionID == id && bs.UserID == userId);
            }
            catch (SqlException ex) when (ex.Number == 208) // Invalid object name
            {
                TempData["ErrorMessage"] = "Bảng BookSuggestions chưa được tạo trong database. Vui lòng chạy script SQL để tạo bảng.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu: {ex.Message}";
                return RedirectToAction("Index");
            }

            if (suggestion == null)
            {
                return NotFound();
            }

            return View(suggestion);
        }
    }
}

