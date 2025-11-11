using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models.ViewModels;
using SchoolLibrary.Web.Services;

namespace SchoolLibrary.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly LibraryDbContext _context;

        public AuthController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: Login
        public IActionResult Login(string? returnUrl = null)
        {
            // Nếu đã đăng nhập, redirect về dashboard tương ứng
            var role = HttpContext.Session.GetString("Role");
            if (!string.IsNullOrEmpty(role))
            {
                return role switch
                {
                    "Admin" => RedirectToAction("Dashboard", "Admin", new { area = "Admin" }),
                    "Thủ thư" or "Librarian" => RedirectToAction("Index", "Home", new { area = "Librarian" }),
                    _ => View(new LoginViewModel { ReturnUrl = returnUrl })
                };
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var normalizedUsername = (model.Username ?? string.Empty).Trim();

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == normalizedUsername && u.IsActive);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng");
                return View(model);
            }

            if (!HashService.VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng");
                return View(model);
            }

            // Lưu thông tin user vào session
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Role", user.Role?.RoleName ?? "");

            if (model.RememberMe)
            {
                HttpContext.Session.SetInt32("RememberMe", 1);
            }

            if (!string.IsNullOrEmpty(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            // Redirect based on role
            return user.RoleID switch
            {
                4 => RedirectToAction("Dashboard", "Admin", new { area = "Admin" }), // Admin
                3 => RedirectToAction("Index", "Home", new { area = "Librarian" }), // Librarian
                2 => RedirectToAction("Index", "Home", new { area = "Teacher" }), // Teacher
                _ => RedirectToAction("Index", "Home", new { area = "Student" }) // Student
            };
        }

        // POST & GET: Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}

