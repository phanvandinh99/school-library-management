using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Services;

namespace SchoolLibrary.Web.Controllers
{
    // Controller helper để fix password - CHỈ DÙNG TRONG DEVELOPMENT
    public class HelperController : Controller
    {
        private readonly LibraryDbContext _context;

        public HelperController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: FixAdminPassword - Tự động set password cho admin
        // URL: /Helper/FixAdminPassword?password=123456
        public IActionResult FixAdminPassword(string? password = "123456")
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == "admin");
            
            if (user == null)
            {
                return Content("Không tìm thấy user 'admin' trong database!");
            }

            user.PasswordHash = HashService.HashPassword(password);
            user.IsActive = true;
            _context.Update(user);
            _context.SaveChanges();

            return Content($"✅ Đã cập nhật password cho admin thành công!\n\nUsername: admin\nPassword: {password}\nHash: {user.PasswordHash}\n\nBây giờ bạn có thể đăng nhập với:\n- Username: admin\n- Password: {password}");
        }

        // GET: GenerateHash - Generate hash cho password
        // URL: /Helper/GenerateHash?password=123456
        public IActionResult GenerateHash(string? password = "123456")
        {
            if (string.IsNullOrEmpty(password))
            {
                return Content("Vui lòng cung cấp password. Ví dụ: /Helper/GenerateHash?password=123456");
            }

            var hash = HashService.HashPassword(password);
            return Content($"Password: {password}\nHash: {hash}\n\nSQL Update:\nUPDATE Users SET PasswordHash = '{hash}' WHERE Username = 'admin';");
        }

        // GET: CheckUser - Kiểm tra user trong database
        // URL: /Helper/CheckUser?username=admin
        public IActionResult CheckUser(string? username = "admin")
        {
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return Content($"❌ Không tìm thấy user: '{username}'");
            }

            var result = $"✅ Tìm thấy user:\n" +
                        $"Username: {user.Username}\n" +
                        $"FullName: {user.FullName}\n" +
                        $"RoleID: {user.RoleID}\n" +
                        $"RoleName: {user.Role?.RoleName ?? "null"}\n" +
                        $"IsActive: {user.IsActive}\n" +
                        $"PasswordHash: {user.PasswordHash}\n\n" +
                        $"Để fix password, truy cập: /Helper/FixAdminPassword?password=123456";

            return Content(result);
        }
    }
}
