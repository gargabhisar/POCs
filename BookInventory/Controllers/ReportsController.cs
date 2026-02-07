using BookInventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Controllers
{
    public class ReportsController : Controller
    {
        private readonly InvoiceService _invoiceService;
        private readonly InvoicePdfService _pdfService;

        public ReportsController(InvoiceService invoiceService, InvoicePdfService pdfService)
        {
            _invoiceService = invoiceService;
            _pdfService = pdfService;
        }

        public IActionResult BookSales(string book)
        {
            ViewBag.BookList = _invoiceService.GetBooksInInvoices();
            ViewBag.SelectedBook = book;

            var data = _invoiceService.GetAll(book);
            return View(data);
        }

        public IActionResult BookSalesPdf(string book, int sortColumnIndex = -1, string sortDirection = "asc")
        {
            var data = _invoiceService.GetAll(book);

            // apply sorting ONLY if UI sorted
            if (sortColumnIndex >= 0)
                data = _invoiceService.SortBookSales(data, sortColumnIndex, sortDirection);

            var pdfBytes = _pdfService.GenerateBookSalesInvoicePdf(data);

            return File(pdfBytes, "application/pdf", $"BookSales_{book ?? "All"}_{DateTime.Now:yyyyMMdd}.pdf");
        }        
    }
}