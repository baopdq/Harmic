using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Harmic.Pages
{
    public class OrderModel : PageModel
    {
        private const string SessionKey = "Cart";

        public List<CartItem> Items { get; set; } = new();

        public decimal Total => Items.Sum(i => i.Price * i.Quantity);

        [BindProperty]
        public OrderInput Input { get; set; } = new();

        public OrderConfirmation? Confirmation { get; set; }

        public void OnGet()
        {
            Items = HttpContext.Session.GetObjectFromJson<List<CartItem>>(SessionKey) ?? new List<CartItem>();
        }

        public IActionResult OnPost()
        {
            Items = HttpContext.Session.GetObjectFromJson<List<CartItem>>(SessionKey) ?? new List<CartItem>();
            if (!Items.Any())
            {
                ModelState.AddModelError(string.Empty, "Giỏ hàng rỗng.");
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Không có DB: giả lập tạo đơn hàng, xoá giỏ hàng
            var orderId = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}";
            Confirmation = new OrderConfirmation
            {
                OrderId = orderId,
                Name = Input.Name,
                Phone = Input.Phone,
                Address = Input.Address,
                Total = Total
            };

            // Clear cart
            HttpContext.Session.Remove(SessionKey);

            // Hiển thị kết quả trên cùng trang
            return Page();
        }
    }

    public class OrderInput
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public class OrderConfirmation
    {
        public string? OrderId { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public decimal Total { get; set; }
    }
}