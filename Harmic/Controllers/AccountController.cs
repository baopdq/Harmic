using Harmic.Models;
using Harmic.Ultilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Harmic.Controllers
{
    public class AccountController : Controller
    {
        private readonly HarmicContext _context;

        public AccountController(HarmicContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginRegister());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRegister model, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(model.LoginUsername) || string.IsNullOrEmpty(model.LoginPassword))
            {
                Function._Message = "Vui lòng nhập đầy đủ tài khoản và mật khẩu.";
                return View("Index", model);
            }

            string passwordHash = HashMD5.GetMD5(model.LoginPassword);
            var account = _context.TbAccounts
                .FirstOrDefault(a => a.Username == model.LoginUsername && a.Password == passwordHash);

            if (account == null)
            {
                Function._Message = "Tài khoản hoặc mật khẩu không đúng.";
                return View("Index", model);
            }

            // Tạo claims và đăng nhập như cũ
            var roleName = _context.TbRoles
                .FirstOrDefault(r => r.RoleId == account.RoleId)?.RoleName ?? "User";

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, account.Username),
            new Claim("AccountId", account.AccountId.ToString()),
            new Claim(ClaimTypes.Role, roleName)
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Register(LoginRegister model)
        {
            var account = model.RegisterAccount;

            if (account == null || string.IsNullOrEmpty(account.Username) || string.IsNullOrEmpty(account.Password))
            {
                Function._Message = "Vui lòng nhập đầy đủ thông tin đăng ký.";
                return View("Index", model);
            }

            if (_context.TbAccounts.Any(a => a.Username == account.Username))
            {
                Function._Message = "Tài khoản đã tồn tại.";
                return View("Index", model);
            }

            account.Password = HashMD5.GetMD5(account.Password);
            account.RoleId = 2; // mặc định là User, hoặc cho chọn

            _context.TbAccounts.Add(account);
            _context.SaveChanges();

            Function._Message = "Đăng ký thành công! Bạn có thể đăng nhập ngay.";
            return View("Index", new LoginRegister()); // reset form
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
    }
}
