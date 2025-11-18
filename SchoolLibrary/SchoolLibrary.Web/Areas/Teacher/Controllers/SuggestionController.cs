using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models;
using System;

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
                TempData["ErrorMessage"] = "Bảng BookSuggestions chưa được tạo trong database. Vui lòng chạy script SQL để tạo bảng.";
                suggestions = new List<BookSuggestion>();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu: {ex.Message}";
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

