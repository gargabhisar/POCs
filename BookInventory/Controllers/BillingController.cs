using BookInventory.Filters;
using BookInventory.Models;
using BookInventory.Repositories;
using BookInventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Controllers
{
    [AuthorizeRole("Admin")]
    public class BillingController : Controller
    {
        private readonly BookService _bookService;
        private readonly InvoiceService _invoiceService;
        private readonly InvoicePdfService _pdfService;
        private readonly CounterRepository _counterRepo;

        public BillingController(
        BookService bookService,
        InvoiceService invoiceService,
        InvoicePdfService pdfService,
        CounterRepository counterRepo)
        {
            _bookService = bookService;
            _invoiceService = invoiceService;
            _pdfService = pdfService;
            _counterRepo = counterRepo;
        }

        // ===============================
        // CREATE BILL (GET)
        // ===============================
        public IActionResult Create()
        {
            var books = _bookService.GetAll();

            ViewBag.Books = books.OrderBy(b => b.Title).ToList();

            return View(new Invoice
            {
                InvoiceDate = DateTime.Now,
                Items = new List<InvoiceItem>()
            });
        }

        // ===============================
        // CREATE BILL (POST)
        // ===============================
        [HttpPost]
        public IActionResult Create(Invoice invoice)
        {
            if (invoice.Items == null || !invoice.Items.Any())
            {
                ModelState.AddModelError("", "Invoice must have at least one item.");
                return View(invoice);
            }

            // 🔹 Calculate totals
            foreach (var item in invoice.Items)
            {
                var book = _bookService.Get(item.BookId);
                if (book == null)
                    return BadRequest("Invalid book selected.");

                item.Title = book.Title;
                item.MRP = book.MRP;

                // 🔒 HARD LIMIT DISCOUNT
                item.DiscountPercent = Math.Clamp((int)item.DiscountPercent, 0, 100);
            }

            invoice.InvoiceDate = DateTime.Now;
            invoice.GrandTotal = invoice.Items.Sum(i => i.LineTotal);

            invoice.InvoiceNo = _counterRepo.GetNextInvoiceNumber();

            // 🔹 Save invoice to DB
            _invoiceService.Save(invoice);

            // 🔹 Generate PDF
            var pdfBytes = _pdfService.GenerateInvoicePdf(invoice);

            var safeName = SanitizeFileName(invoice.CustomerName);
            var datePart = invoice.InvoiceDate.ToString("yyyy-MM-dd_HH-mm-ss");

            var fileName = $"{invoice.CustomerMobile}_{safeName}_{datePart}_INV-{invoice.InvoiceNo:D4}.pdf";

            // 🔐 Store in Session
            HttpContext.Session.Set("LastInvoicePdf", pdfBytes);
            HttpContext.Session.SetString("LastInvoiceFileName", fileName);

            TempData["Success"] = "Invoice generated successfully.";

            // 🔑 IMPORTANT
            return RedirectToAction("Create", new { download = 1 });
        }

        public IActionResult DownloadLastInvoice()
        {
            var pdf = HttpContext.Session.Get("LastInvoicePdf");
            var fileName = HttpContext.Session.GetString("LastInvoiceFileName");

            if (pdf == null || fileName == null)
                return NotFound();

            HttpContext.Session.Remove("LastInvoicePdf");
            HttpContext.Session.Remove("LastInvoiceFileName");

            return File(pdf, "application/pdf", fileName);
        }

        [AuthorizeRole("Admin")]
        public IActionResult List()
        {
            var invoices = _invoiceService.GetAll();
            return View(invoices);
        }

        [AuthorizeRole("Admin")]
        public IActionResult Download(string id)
        {
            var invoice = _invoiceService.Get(id);
            if (invoice == null)
                return NotFound();

            var pdfBytes = _pdfService.GenerateInvoicePdf(invoice);

            var safeName = SanitizeFileName(invoice.CustomerName);
            var datePart = invoice.InvoiceDate.ToString("yyyy-MM-dd_HH-mm-ss");

            var fileName = $"{invoice.CustomerMobile}_{safeName}_{datePart}_INV-{invoice.InvoiceNo:D4}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        // ===============================
        // HELPERS
        // ===============================
        private string SanitizeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Customer";

            return string.Concat(
                input.Where(char.IsLetterOrDigit)
            );
        }

    }
}
