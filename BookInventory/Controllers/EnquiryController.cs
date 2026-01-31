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
        public IActionResult List(string search, int page = 1)
        {
            const int pageSize = 10;

            var all = _repo.GetAll(); // already sorted by SerialNo

            // 🔍 SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                all = all.Where(x =>
                    (!string.IsNullOrEmpty(x.Name) && x.Name.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(x.Mobile) && x.Mobile.Contains(search)) ||
                    (!string.IsNullOrEmpty(x.Email) && x.Email.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(x.Comments) && x.Comments.ToLower().Contains(search))
                ).ToList();
            }

            // 📄 PAGINATION
            var totalCount = all.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var data = all
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;

            return View(data);
        }
    }
}
