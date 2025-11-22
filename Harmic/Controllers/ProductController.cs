using Harmic.Models;
using Harmic.Ultilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harmic.Controllers
{
    public class ProductController : Controller
    {
        private readonly HarmicContext _context;
        public ProductController(HarmicContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Route("/product/{alias}-{id}.html")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var product = await _context.TbProducts.Include(i => i.CategoryProduct).FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("CART");
            ViewBag.CartCount = cart?.Sum(x => x.Quantity) ?? 0;
            ViewBag.productReview = _context.TbProductReviews.Where(i => i.ProductId == id && i.IsActive).ToList();
            ViewBag.productRelated = _context.TbProducts.Where(i => i.CategoryProductId != product.CategoryProductId && i.ProductId != product.ProductId).Take(4).ToList();
            return View(product);
        }
    }
}
