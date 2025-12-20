using BookInventory.Helpers;
using BookInventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DashboardService _service;

        public DashboardController(DashboardService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            var user = SessionHelper.GetUser(HttpContext);
            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewBag.TotalBooks = _service.TotalBooks();
            ViewBag.TotalQuantity = _service.TotalQuantity();
            ViewBag.Almirah = _service.AlmirahCount();
            ViewBag.Bed = _service.BedCount();
            ViewBag.Box = _service.BoxCount();
            ViewBag.Other = _service.OtherCount();
            ViewBag.LowStock = _service.LowStock();

            return View();
        }
    }
}
