using Harmic.Models;
using Harmic.Ultilities;
using Microsoft.AspNetCore.Mvc;

namespace Harmic.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RegisterController : Controller
    {
        private readonly HarmicContext _context;
        public RegisterController(HarmicContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Index(TbAccount account)
        {
            if (account == null)
            {
                return NotFound();
            }
            var check = _context.TbAccounts.FirstOrDefault(a => a.Username == account.Username);
            if (check != null)
            {
                Function._Message = "Tài khoản đã tồn tại.";
                return RedirectToAction("Index", "Register");
            }
            Function._Message = "Đăng ký thành công.";
            account.Password = HashMD5.GetMD5(account.Password != null ? account.Password : "");
            _context.TbAccounts.Add(account);
            _context.SaveChanges();
            return RedirectToAction("Index", "Login");
        }
    }

}
