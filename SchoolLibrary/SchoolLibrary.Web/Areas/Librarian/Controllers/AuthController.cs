using Microsoft.AspNetCore.Mvc;

namespace SchoolLibrary.Web.Areas.Librarian.Controllers
{
    [Area("Librarian")]
    public class AuthController : Controller
    {
        // GET: Login - Redirect về trang login chung
        public IActionResult Login(string? returnUrl = null)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = returnUrl });
        }

        // POST & GET: Logout - Xóa session và redirect về controller chung
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }
    }
}

