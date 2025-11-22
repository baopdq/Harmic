using Harmic.Models;
using Harmic.Ultilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Harmic.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoginController : Controller
    {
        private readonly HarmicContext _context;
        private string? returnUrl;

        public LoginController(HarmicContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index( string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index(TbAccount account)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (string.IsNullOrEmpty(account?.Username) || string.IsNullOrEmpty(account?.Password))
            {
                Function._Message = "Vui lòng nhập đầy đủ tài khoản và mật khẩu.";
                return View();
            }

            string passwordHash = HashMD5.GetMD5(account.Password);
            var check = _context.TbAccounts
                                .FirstOrDefault(a => a.Username == account.Username
                                                  && a.Password == passwordHash);

            if (check == null)
            {
                Function._Message = "Tài khoản hoặc mật khẩu không đúng.";
                return View();
            }
            var roleName = _context.TbRoles
            .Where(r => r.RoleId == check.RoleId)
                .Select(r => r.RoleName)
                    .FirstOrDefault() ?? "User";
            // Giả sử RoleId: 1 = Admin, 2 = User
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, check.Username),
        new Claim("AccountId", check.AccountId.ToString()),
        new Claim(ClaimTypes.Role, roleName)
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Nếu có returnUrl thì quay lại đúng trang đó
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            // Về trang Admin/Home
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Denied()
        {
            // Nếu có user đang đăng nhập (User, v.v…) thì sign out luôn
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Thông báo cho người dùng
            Function._Message = "Chỉ tài khoản Admin mới có quyền đăng nhập khu vực này.";

            // Quay lại trang login admin
            return RedirectToAction("Index", "Login", new { area = "Admin" });
        }
    }
}
