using Microsoft.AspNetCore.Mvc;

namespace Harmic.Models
{
    public class Checkout : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
