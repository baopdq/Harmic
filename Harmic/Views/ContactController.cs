using Harmic.Models;
using Microsoft.AspNetCore.Mvc;

namespace Harmic.Views
{
    public class ContactController : Controller
    {
        private readonly HarmicContext _context;
        public ContactController(HarmicContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(string name, string phone, string email, string message)
        {
            try
            {
                TbContact contact = new TbContact();
                contact.Name = name;
                contact.Phone = phone;
                contact.Email = email;
                contact.Message = message;
                contact.CreatedDate = DateTime.Now;
                _context.Add(contact);
                _context.SaveChangesAsync();
                return Json(new { success = true, message = "Gửi liên hệ thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Gửi liên hệ thất bại. Lỗi: " + ex.Message });
            }
        }
    }
}
