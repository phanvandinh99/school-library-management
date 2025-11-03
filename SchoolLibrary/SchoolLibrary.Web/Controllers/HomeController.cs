using Microsoft.AspNetCore.Mvc;

namespace SchoolLibrary.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Login", "Auth", new { area = "Admin" });
        }
    }
}

