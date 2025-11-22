using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Harmic.Ultilities;

namespace Harmic.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
             return View();
        }

        public IActionResult Logout()
        {
            Function._Username = null;
            Function._AccountID = 0;
            Function._Message = "Đăng xuất thành công.";
            return RedirectToAction("Index", "Login");
        }

    }
}
