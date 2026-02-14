using BookInventory.Filters;
using BookInventory.Helpers;
using BookInventory.Models;
using BookInventory.Repositories;
using BookInventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly UserRepository _userRepository;

        public AccountController(AuthService authService, UserRepository userRepository)
        {
            _authService = authService;
            _userRepository = userRepository;
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

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordVM model)
        {
            var userCheck = SessionHelper.GetUser(HttpContext);

            if (userCheck == null)
                return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(model.OldPassword) ||
                string.IsNullOrWhiteSpace(model.NewPassword))
            {
                TempData["Error"] = "All fields are required.";
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["Error"] = "New passwords do not match.";
                return View(model);
            }

            var user = _userRepository.GetByEmail(userCheck.Email);

            // 🔐 Verify old password
            var result = _authService.VerifyPassword(user, model.OldPassword);
            if (!result)
            {
                TempData["Error"] = "Old password is incorrect.";
                return View(model);
            }

            // 🔐 Update password
            _authService.UpdatePassword(user.Email, model.NewPassword);

            // 🔐 Logout after password change (recommended)
            SessionHelper.Logout(HttpContext);

            TempData["SuccessPassword"] = "Password changed successfully. Please login again.";
            return RedirectToAction("Login");
        }
    }
}
