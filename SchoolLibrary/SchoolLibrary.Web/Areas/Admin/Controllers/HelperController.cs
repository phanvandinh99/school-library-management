using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Services;

namespace SchoolLibrary.Web.Areas.Admin.Controllers
{
    // Controller chỉ dùng cho mục đích development
    // XÓA controller này trước khi deploy production
    [Area("Admin")]
    public class HelperController : Controller
    {
        private readonly LibraryDbContext _context;

        public HelperController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: Generate hash for password
        // Example: /Admin/Helper/GenerateHash?password=123456
        public IActionResult GenerateHash(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return Content("Vui lòng cung cấp password. Ví dụ: /Admin/Helper/GenerateHash?password=123456");
            }

            var hash = HashService.HashPassword(password);
            return Content($"Password: {password}\nHash: {hash}\n\nCập nhật database với hash này để đăng nhập được.");
        }

        // GET: Diagnostics DB and user info
        // Example: /Admin/Helper/Diag?username=admin
        public IActionResult Diag(string? username, string? set)
        {
            var normalizedUsername = (username ?? "admin").Trim();

            var conn = _context.Database.GetDbConnection();
            var server = conn.DataSource ?? string.Empty;
            var database = conn.Database ?? string.Empty;

            var user = _context.Users.FirstOrDefault(u => u.Username == normalizedUsername);

            // If `set` is provided, update the user's password to this value (dev-only)
            if (!string.IsNullOrWhiteSpace(set) && user != null)
            {
                user.PasswordHash = HashService.HashPassword(set);
                user.IsActive = true;
                _context.Update(user);
                _context.SaveChanges();
            }

            var result = $"Server: {server}\n" +
                         $"Database: {database}\n" +
                         $"Queried Username: '{normalizedUsername}'\n" +
                         $"Found: {(user != null)}\n" +
                         $"IsActive: {(user?.IsActive.ToString() ?? "null")}\n" +
                         $"RoleID: {(user?.RoleID.ToString() ?? "null")}\n" +
                         $"PasswordHash: {(user?.PasswordHash ?? "null")}\n";

            return Content(result);
        }

        // GET: Force set password for a username (dev-only)
        // Example: /Admin/Helper/SetAdminPassword?username=admin&password=Abc123456@
        public IActionResult SetAdminPassword(string? username, string? password)
        {
            var normalizedUsername = (username ?? "admin").Trim();
            if (string.IsNullOrWhiteSpace(password))
            {
                return Content("Vui lòng cung cấp password. Ví dụ: /Admin/Helper/SetAdminPassword?username=admin&password=Abc123456@");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == normalizedUsername);
            if (user == null)
            {
                return Content($"Không tìm thấy user: '{normalizedUsername}'");
            }

            user.PasswordHash = HashService.HashPassword(password);
            user.IsActive = true;
            _context.Update(user);
            _context.SaveChanges();

            var conn = _context.Database.GetDbConnection();
            var server = conn.DataSource ?? string.Empty;
            var database = conn.Database ?? string.Empty;
            var hash = user.PasswordHash;

            return Content($"Server: {server}\nDatabase: {database}\nUpdated Username: '{normalizedUsername}'\nNew PasswordHash: {hash}\nDone.");
        }

        // GET: Test connection
        public IActionResult TestConnection()
        {
            return Content("HelperController đang hoạt động. Đừng quên xóa controller này trước khi deploy!");
        }
    }
}

