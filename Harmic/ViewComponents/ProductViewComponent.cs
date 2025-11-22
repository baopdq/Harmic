using Harmic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Harmic.ViewComponents
{
    public class ProductViewComponent : ViewComponent
    {
        private readonly HarmicContext _context;
        public ProductViewComponent(HarmicContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync(int take)
        {
            var items = _context.TbProducts.Include(p => p.CategoryProduct)
                .Where(p => (bool)p.IsActive).Where(p => (bool)p.IsNew);
                
            return await Task.FromResult<IViewComponentResult>(View(items.OrderByDescending(m => m.ProductId).ToList()));
        }
    }
}
