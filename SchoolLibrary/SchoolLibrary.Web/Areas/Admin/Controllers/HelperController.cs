using Microsoft.AspNetCore.Mvc;
using SchoolLibrary.Web.Services;

namespace SchoolLibrary.Web.Areas.Admin.Controllers
{
    // Controller chỉ dùng cho mục đích development
    // XÓA controller này trước khi deploy production
    public class HelperController : Controller
    {
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

        // GET: Test connection
        public IActionResult TestConnection()
        {
            return Content("HelperController đang hoạt động. Đừng quên xóa controller này trước khi deploy!");
        }
    }
}

