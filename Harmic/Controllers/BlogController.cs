using Harmic.Models;
using Harmic.Ultilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harmic.Controllers
{
   
    public class BlogController : Controller
    {
        private readonly HarmicContext _context;
        public BlogController(HarmicContext context)
        {
            _context = context;
        }      
        public IActionResult Index()
        {
            return View();
        }
        [Route("/blog/{alias}-{id}.html")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TbBlogs == null)
            {
                return NotFound();
            }
            var blog = await _context.TbBlogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }
            var blogs = await _context.TbBlogs.FirstOrDefaultAsync(m => m.BlogId == id);
            if (blogs == null) {
                return NotFound();
            }
            ViewBag.blogComment = _context.TbBlogComments.Where(i => i.BlogId == id && i.IsActive).ToList();
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("CART");
            ViewBag.CartCount = cart?.Sum(x => x.Quantity) ?? 0;

            return View(blog);
        }
    }
}
