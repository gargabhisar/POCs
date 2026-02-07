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

        public IActionResult BookSalesPdf(string book, string paymentMode, int sortColumnIndex, string sortDirection)
        {
            var data = _invoiceService.GetAll(book);

            // ✅ APPLY PAYMENT MODE FILTER
            if (!string.IsNullOrWhiteSpace(paymentMode))
            {
                data = data
                    .Where(x => x.PaymentMode == paymentMode)
                    .ToList();
            }

            // ✅ APPLY SORTING ONLY IF UI SORTED
            if (sortColumnIndex >= 0)
            {
                data = _invoiceService.SortBookSales(
                    data,
                    sortColumnIndex,
                    sortDirection
                );
            }

            var pdfBytes = _pdfService.GenerateBookSalesInvoicePdf(data);

            return File(
                pdfBytes,
                "application/pdf",
                $"BookSales_{book ?? "All"}_{paymentMode ?? "All"}_{DateTime.Now:yyyyMMdd}.pdf"
            );
        }
    }
}