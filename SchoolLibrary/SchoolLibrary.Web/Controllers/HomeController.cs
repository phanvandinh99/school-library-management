using Microsoft.AspNetCore.Mvc;

namespace SchoolLibrary.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Kiểm tra nếu đã đăng nhập, redirect về dashboard tương ứng
            var role = HttpContext.Session.GetString("Role");
            if (!string.IsNullOrEmpty(role))
            {
                return role switch
                {
                    "Admin" => RedirectToAction("Dashboard", "Admin", new { area = "Admin" }),
                    "Thủ thư" or "Librarian" => RedirectToAction("Index", "Home", new { area = "Librarian" }),
                    _ => RedirectToAction("Login", "Auth")
                };
            }

            return RedirectToAction("Login", "Auth");
        }
    }
}

