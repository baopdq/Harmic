using Harmic.Models;
using Harmic.Ultilities;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Harmic.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HarmicContext _context;

        public HomeController(HarmicContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.productCategories = _context.TbProductCategories.ToList();
            ViewBag.productNew = _context.TbProducts.Where(m => m.IsNew).ToList();
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("CART");
            ViewBag.CartCount = cart?.Sum(x => x.Quantity) ?? 0;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
