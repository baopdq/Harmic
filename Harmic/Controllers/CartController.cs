using Harmic.Models;
using Microsoft.AspNetCore.Mvc;
using Harmic.Ultilities;
using Microsoft.AspNetCore.Authorization;
using Harmic.Services;

namespace Harmic.Controllers
{
    public class CartController : Controller
    {
        private readonly HarmicContext _context;
        private readonly IVnPayService _vnPayService;
        private const string SessionCartKey = "Cart";

        public CartController(HarmicContext context, IVnPayService vnPayService)
        {
            _context = context;
            _vnPayService = vnPayService;
        }

        // ================== Helper ==================

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

        // ================== Action ==================

        public IActionResult Index()
        {
            return View(GetCart());
        }

        [HttpGet]
        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var product = _context.TbProducts.Find(id);
            if (product == null) return NotFound();

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
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (cartItem != null)
            {
                if (quantity <= 0)
                    cart.Remove(cartItem);
                else
                    cartItem.Quantity = quantity;

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
            if (!cart.Any())
            {
                return RedirectToAction("Index");
            }
            return View(cart);
        }

        [HttpPost]
        public IActionResult Checkout(string name, string email, string phone, string address, string payment = "COD")
        {
            var cart = GetCart();
            if (!cart.Any()) return RedirectToAction("Index");

            // Nếu chọn VNPAY thì redirect sang VNPAY, không tạo đơn (đơn giản hóa flow)
            if (payment == "Thanh Toán (VNPAY)")
            {
                var totalAmount = (int)cart.Sum(p => p.Price * p.Quantity);

                var vnPayModel = new VnPaymentRequestModel
                {
                    Amount = totalAmount,
                    CreatedDate = DateTime.Now,
                    Descripton = $"{name} {phone}",
                    Fullname = name,
                    OrderId = new Random().Next(1000, 100000)
                };

                var url = _vnPayService.CreatePaymentUrl(HttpContext, vnPayModel);
                Console.WriteLine("VNPAY URL: " + url);
                return Redirect(url);
            }

            // ======== Thanh toán thường (COD) – tạo đơn hàng trong DB ========

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
            int totalAmountCod = (int)cart.Sum(item => item.Price * item.Quantity);

            var orderSummary = new TbOrder
            {
                CustomerName = name,
                Phone = phone,
                Address = address,
                Quantity = totalQuantity,
                TotalAmount = totalAmountCod,
                CreatedDate = DateTime.Now,
                OrderStatusId = 1 // 1 = Pending
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

            // 4. Xóa giỏ & về trang chủ
            SaveCart(new List<CartItem>());
            TempData["Message"] = "Đặt hàng thành công!";
            return RedirectToAction("Index", "Home");
        }

        // ================== Callback VNPAY ==================

        [AllowAnonymous]
        public IActionResult PaymentCallBack()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response == null)
            {
                TempData["Message"] = "Không nhận được kết quả thanh toán từ VNPAY.";
                return RedirectToAction("Index", "Home");
            }

            if (response.VnPayResponseCode != "00" || !response.Success)
            {
                TempData["Message"] = $"Thanh toán thất bại: {response.VnPayResponseCode}";
                return RedirectToAction("Index");  // quay về giỏ
            }

            // Thanh toán thành công
            SaveCart(new List<CartItem>());
            TempData["Message"] = "Thanh toán VNPAY thành công!";
            return RedirectToAction("Index", "Home");
        }
    }
}
