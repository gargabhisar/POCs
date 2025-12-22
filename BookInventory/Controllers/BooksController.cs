using BookInventory.Filters;
using BookInventory.Helpers;
using BookInventory.Models;
using BookInventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Controllers
{
    public class BooksController : Controller
    {
        private readonly BookService _service;

        public BooksController(BookService service)
        {
            _service = service;
        }

        // LIST + SEARCH
        public IActionResult Index(string search)
        {
            if (SessionHelper.GetUser(HttpContext) == null)
                return RedirectToAction("Login", "Account");

            var books = _service.Search(search);
            ViewBag.Search = search;
            return View(books);
        }

        // CREATE
        [AuthorizeRole("Admin")]
        public IActionResult Create()
        {
            return View(new Book
            {
                SerialNo = _service.GetNextSerialNo(), // ✅ auto-generate
                Locations = new BookLocations
                {
                    Other = new OtherLocation()
                }
            });
        }

        [AuthorizeRole("Admin")]
        [HttpPost]
        public IActionResult Create(Book book)
        {
            if (!ValidateBook(book))
                return View(book);

            // 🔒 Enforce backend-generated SerialNo
            book.SerialNo = _service.GetNextSerialNo();

            _service.Save(book);
            return RedirectToAction("Index");
        }

        // EDIT
        [AuthorizeRole("Admin")]
        public IActionResult Edit(string id)
        {
            var book = _service.Get(id);
            if (book == null)
                return NotFound();

            return View(book);
        }

        [AuthorizeRole("Admin")]
        [HttpPost]
        public IActionResult Edit(Book book)
        {
            if (!ValidateBook(book))
                return View(book);

            // 🔒 Preserve original SerialNo
            var existing = _service.Get(book.Id.ToString());
            if (existing == null)
                return NotFound();

            book.SerialNo = existing.SerialNo;

            _service.Save(book);
            return RedirectToAction("Index");
        }

        // VALIDATION
        private bool ValidateBook(Book book)
        {
            if (string.IsNullOrWhiteSpace(book.Title))
            {
                ModelState.AddModelError("", "Title is required");
                return false;
            }

            if (book.Locations.Other.Quantity > 0 &&
                string.IsNullOrWhiteSpace(book.Locations.Other.Name))
            {
                ModelState.AddModelError("", "Other location name required");
                return false;
            }

            return true;
        }
    }
}