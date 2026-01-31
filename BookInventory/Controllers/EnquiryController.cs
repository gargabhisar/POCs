using BookInventory.Filters;
using BookInventory.Models;
using BookInventory.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Controllers
{
    [AuthorizeRole("Admin")]
    public class EnquiryController : Controller
    {
        private readonly EnquiryRepository _repo;
        private readonly CounterRepository _counterRepo;

        public EnquiryController(
            EnquiryRepository repo,
            CounterRepository counterRepo)
        {
            _repo = repo;
            _counterRepo = counterRepo;
        }

        // DATA ENTRY
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Enquiry enquiry)
        {
            if (string.IsNullOrWhiteSpace(enquiry.Name) ||
                string.IsNullOrWhiteSpace(enquiry.Mobile))
            {
                ModelState.AddModelError("", "Name and Mobile are required.");
                return View(enquiry);
            }

            enquiry.SerialNo = _counterRepo.GetNext("EnquirySerialNo");
            enquiry.CreatedAt = DateTime.Now;

            _repo.Insert(enquiry);

            TempData["Success"] = "Enquiry saved successfully.";
            return RedirectToAction("Create");
        }

        // LIST VIEW
        public IActionResult List()
        {
            var enquiries = _repo.GetAll();
            return View(enquiries);
        }
    }
}
