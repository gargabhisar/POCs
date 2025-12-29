using BookInventory.Helpers;
using BookInventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;

        public AccountController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var user = SessionHelper.GetUser(HttpContext);

            if (user != null)
            {
                // Already logged in → go to dashboard
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required";
                return View();
            }

            var user = _authService.Authenticate(email, password);

            if (user == null)
            {
                ViewBag.Error = "Invalid credentials or inactive user";
                return View();
            }

            SessionHelper.SetUser(HttpContext, user);
            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
