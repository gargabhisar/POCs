using BookInventory.Filters;
using BookInventory.Models;
using BookInventory.Repositories;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;

namespace BookInventory.Controllers
{
    [AuthorizeRole("Admin")]
    [ServiceFilter(typeof(ClearEnquiryCacheFilter))]
    public class EnquiryController : Controller
    {
        private readonly EnquiryRepository _repo;
        private readonly CounterRepository _counterRepo;

        const string ENQUIRY_SESSION_KEY = "ENQUIRY_LIST_CACHE";

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

            TempData["Success"] = $"Enquiry No. {enquiry.SerialNo} for {enquiry.Name} saved successfully.";
            return RedirectToAction("Create");
        }

        // LIST VIEW
        public IActionResult List(string search, int page = 1)
        {
            const int pageSize = 10;

            List<Enquiry> all;

            // 🔹 Load from Session if exists
            var cached = HttpContext.Session.GetString(ENQUIRY_SESSION_KEY);

            if (cached == null)
            {
                all = _repo.GetAll(); // DB hit only once
                HttpContext.Session.SetString(
                    ENQUIRY_SESSION_KEY,
                    System.Text.Json.JsonSerializer.Serialize(all)
                );
            }
            else
            {
                all = System.Text.Json.JsonSerializer.Deserialize<List<Enquiry>>(cached);
            }

            // 🔍 SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
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

        [AuthorizeRole("Admin")]
        public IActionResult Export(string search)
        {
            var data = _repo.GetAll();

            // Apply same search logic as List
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                data = data.Where(x =>
                    (!string.IsNullOrEmpty(x.Name) && x.Name.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(x.Mobile) && x.Mobile.Contains(search)) ||
                    (!string.IsNullOrEmpty(x.Email) && x.Email.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(x.Comments) && x.Comments.ToLower().Contains(search))
                ).ToList();
            }

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Enquiries");

            // Header
            ws.Cell(1, 1).Value = "S.No";
            ws.Cell(1, 2).Value = "Name";
            ws.Cell(1, 3).Value = "Mobile";
            ws.Cell(1, 4).Value = "Email";
            ws.Cell(1, 5).Value = "Comments";

            ws.Range("A1:E1").Style.Font.Bold = true;

            int row = 2;
            foreach (var e in data)
            {
                ws.Cell(row, 1).Value = e.SerialNo;
                ws.Cell(row, 2).Value = e.Name;
                ws.Cell(row, 3).Value = e.Mobile;
                ws.Cell(row, 4).Value = e.Email;
                ws.Cell(row, 5).Value = e.Comments;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            var fileName = $"Enquiries_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
    }
}
