using BookInventory.Filters;
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
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required";
                return View();
            }

            var user = _authService.Authenticate(email, password);

            if (user == null)
            {
                ViewBag.Error = "Invalid credentials";
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

        [AuthorizeRole("Admin")]
        public IActionResult Register()
        {
            ViewBag.Roles = new List<string> { "Admin", "Viewer" };
            return View();
        }

        [HttpPost]
        [AuthorizeRole("Admin")]
        public IActionResult Register(string name, string email, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(role))
            {
                ViewBag.Error = "All fields are required";
                ViewBag.Roles = new List<string> { "Admin", "Viewer" };
                return View();
            }

            var success = _authService.Register(name, email, password, role);

            if (!success)
            {
                ViewBag.Error = "User already exists";
                return View();
            }

            TempData["UserCreateSuccess"] = $"User '{name}' created as {role}";
            return RedirectToAction("Register", "Account");
        }
    }
}
