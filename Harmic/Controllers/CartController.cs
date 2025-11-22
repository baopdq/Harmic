using Harmic.Models;
using Microsoft.AspNetCore.Mvc;
using Harmic.Ultilities;
namespace Harmic.Controllers
{
    public class CartController : Controller
    {
        private readonly HarmicContext _context;
        private const string SessionCartKey = "Cart";
        private const string SessionCustomerKey = "Customer";
        public CartController(HarmicContext context)
        {
            _context = context;
        }
        private List<CartItem> GetCart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(SessionCartKey);
            if (cart == null)
            {
                cart = new List<CartItem>();
                HttpContext.Session.SetObjectAsJson(SessionCartKey, cart);
            }
            return cart;
        }
        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetObjectAsJson(SessionCartKey, cart);
        }
        public IActionResult Index()
        {

            return View(GetCart());
        }
        [HttpGet]
        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var product = _context.TbProducts.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == id);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.Title,
                    Image = product.Image,
                    Price = product.Price ?? 0,
                    Quantity = quantity
                });
            }

            SaveCart(cart);
            TempData["AddedToCart"] = product.Title + " đã được thêm vào giỏ!";
            return RedirectToAction("Index","Home");
        }

        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == productId);
            if (cartItem != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = quantity;
                }
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == productId);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(SessionCartKey);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (cart.Count == 0)
            {
                return RedirectToAction("Index");
            }

            return View(cart);
        }
        [HttpPost]
        [HttpPost]
        public IActionResult Checkout(string name, string email, string phone, string address)
        {
            var cart = GetCart();
            if (!cart.Any()) return RedirectToAction("Index");

            // 1. Tìm customer cũ theo email/phone, nếu chưa có thì tạo mới
            var customer = _context.TbCustomers
                .FirstOrDefault(c => c.Email == email || c.Phone == phone);

            if (customer == null)
            {
                customer = new TbCustomer
                {
                    Username = email ?? phone,
                    Email = email,
                    Phone = phone,
                    IsActive = true,
                    LastLogin = DateTime.Now
                };
                _context.TbCustomers.Add(customer);
                _context.SaveChanges();
            }

            // 2. Tạo đơn hàng
            int totalQuantity = cart.Sum(item => item.Quantity);
            int totalAmount = (int)cart.Sum(item => item.Total);

            var orderSummary = new TbOrder
            {
                CustomerName = name,
                Phone = phone,
                Address = address,
                Quantity = totalQuantity,
                TotalAmount = totalAmount,
                CreatedDate = DateTime.Now,
                OrderStatusId = 1
            };
            _context.TbOrders.Add(orderSummary);
            _context.SaveChanges();

            // 3. Lưu chi tiết
            foreach (var item in cart)
            {
                var orderDetail = new TbOrderDetail
                {
                    OrderId = orderSummary.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = (int)item.Price
                };
                _context.TbOrderDetails.Add(orderDetail);
            }
            _context.SaveChanges();

            SaveCart(new List<CartItem>());
            return RedirectToAction("OrderSuccess", new { id = orderSummary.OrderId });
        }

        public IActionResult OrderSuccess(int id)
        {
            var order = _context.TbOrders.Find(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }
        public IActionResult OrderCancel(int id)
        {
            var order = _context.TbOrders.Find(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }
        public IActionResult OrderConfirm(int id)
        {
            var order = _context.TbOrders.Find(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }
    }
}
