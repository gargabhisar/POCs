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
        private readonly PublishingServiceService _publishingService;
        private readonly PublishingInvoiceRepository _publishingInvoiceRepo;
        private readonly PublishingInvoicePdfService _publishingPdfService;

        public BillingController(
            BookService bookService,
            InvoiceService invoiceService,
            InvoicePdfService pdfService,
            CounterRepository counterRepo,
            PublishingServiceService publishingService,
            PublishingInvoiceRepository publishingInvoiceRepo,
            PublishingInvoicePdfService publishingPdfService)
        {
            _bookService = bookService;
            _invoiceService = invoiceService;
            _pdfService = pdfService;
            _counterRepo = counterRepo;
            _publishingService = publishingService;
            _publishingInvoiceRepo = publishingInvoiceRepo;
            _publishingPdfService = publishingPdfService;
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

                // 🔒 HARD LIMIT DISCOUNT (INTEGER 0–100)
                item.DiscountPercent = Math.Clamp(item.DiscountPercent, 0, 100);

                // ✅ EXACT SAME LOGIC AS UI
                var finalPrice = Math.Ceiling((decimal)item.MRP - ((decimal)item.MRP * item.DiscountPercent / 100m));

                item.FinalPrice = (int)finalPrice;
                item.LineTotal = item.FinalPrice * item.Quantity;
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

            TempData["Success"] = $"Invoice No. {invoice.InvoiceNo} generated successfully.";

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

        public IActionResult DownloadPublishingLast()
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

        [AuthorizeRole("Admin")]
        public IActionResult CreatePublishing()
        {
            ViewBag.Services = _publishingService.GetActiveServices();
            return View(new PublishingInvoice
            {
                Services = new List<PublishingInvoiceItem>()
            });
        }

        [HttpPost]
        [AuthorizeRole("Admin")]
        public IActionResult CreatePublishing(PublishingInvoice invoice)
        {
            if (invoice.Services == null || !invoice.Services.Any())
            {
                ModelState.AddModelError("", "At least one service is required.");
                ViewBag.Services = _publishingService.GetActiveServices();
                return View(invoice);
            }

            invoice.InvoiceNo = _counterRepo.GetNextInvoiceNumber();
            invoice.InvoiceDate = DateTime.Now;

            decimal subTotal = 0;
            decimal igst = 0, cgst = 0, sgst = 0;

            foreach (var item in invoice.Services)
            {
                var service = _publishingService.Get(item.ServiceId);
                if (service == null || !service.IsActive)
                    return BadRequest("Invalid service selected.");

                // Snapshot service data
                item.ServiceName = service.Name;
                item.BasePrice = service.Price;

                // 🔒 Discount (integer, capped)
                item.DiscountPercent = Math.Clamp(item.DiscountPercent, 0, 100);

                item.DiscountAmount =
                    Math.Ceiling(item.BasePrice * item.DiscountPercent / 100);

                item.TaxableAmount =
                    item.BasePrice - item.DiscountAmount;

                subTotal += item.TaxableAmount;
            }

            // 🔹 TAX CALCULATION (18%)
            if (invoice.TaxType == "IGST")
            {
                igst = Math.Ceiling(subTotal * 0.18m);
            }
            else
            {
                cgst = Math.Ceiling(subTotal * 0.09m);
                sgst = Math.Ceiling(subTotal * 0.09m);
            }

            invoice.IGST = igst;
            invoice.CGST = cgst;
            invoice.SGST = sgst;

            invoice.SubTotal = subTotal;
            invoice.GrandTotal = subTotal + igst + cgst + sgst;

            _publishingInvoiceRepo.Insert(invoice);

            var pdfBytes = _publishingPdfService.Generate(invoice);

            var safeName = SanitizeFileName(invoice.AuthorName);
            var datePart = invoice.InvoiceDate.ToString("yyyy-MM-dd_HH-mm-ss");

            var fileName = $"{invoice.AuthorMobile}_{safeName}_{datePart}_INV-{invoice.InvoiceNo:D4}.pdf";

            // 🔐 Store in Session
            HttpContext.Session.Set("LastInvoicePdf", pdfBytes);
            HttpContext.Session.SetString("LastInvoiceFileName", fileName);

            TempData["Success"] = $"Publishing Invoice No. {invoice.InvoiceNo} generated successfully.";

            return RedirectToAction("CreatePublishing", new { download = 1 });
        }

        [AuthorizeRole("Admin")]
        public IActionResult PublishingList()
        {
            var invoices = _publishingInvoiceRepo
                .GetAll()
                .OrderByDescending(x => x.InvoiceNo)
                .ToList();

            return View(invoices);
        }

        [AuthorizeRole("Admin")]
        public IActionResult DownloadPublishing(string id)
        {
            var invoice = _publishingInvoiceRepo.GetById(id);
            if (invoice == null)
                return NotFound();

            var pdfBytes = _publishingPdfService.Generate(invoice);

            var safeName = SanitizeFileName(invoice.AuthorName);
            var datePart = invoice.InvoiceDate.ToString("yyyy-MM-dd_HH-mm-ss");

            var fileName =
                $"{invoice.AuthorMobile}_{safeName}_{datePart}_INV-{invoice.InvoiceNo:D4}.pdf";

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
