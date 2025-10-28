using Microsoft.AspNetCore.Mvc;

namespace SchoolLibrary.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Redirect to login page if user is not authenticated
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "Admin" });
            }

            // If authenticated, redirect based on role
            var role = HttpContext.Session.GetString("Role");
            return role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin", new { area = "Admin" }),
                "Librarian" => RedirectToAction("Index", "Home", new { area = "Librarian" }),
                "Teacher" => RedirectToAction("Index", "Home", new { area = "Teacher" }),
                "Student" => RedirectToAction("Index", "Home", new { area = "Student" }),
                _ => RedirectToAction("Login", "Auth", new { area = "Admin" })
            };
        }

    }
}

