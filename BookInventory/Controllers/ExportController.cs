using BookInventory.Helpers;
using BookInventory.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Controllers
{
    public class ExportController : Controller
    {
        private readonly BookService _service;

        public ExportController(BookService service)
        {
            _service = service;
        }

        public IActionResult Books()
        {
            if (SessionHelper.GetUser(HttpContext) == null)
                return RedirectToAction("Login", "Account");

            var books = _service.GetAll();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Books");

            // HEADER
            ws.Cell(1, 1).Value = "Title";
            ws.Cell(1, 2).Value = "Author";
            ws.Cell(1, 3).Value = "ISBN";
            ws.Cell(1, 4).Value = "Total Qty";
            ws.Cell(1, 5).Value = "Almirah";
            ws.Cell(1, 6).Value = "Bed";
            ws.Cell(1, 7).Value = "Box";
            ws.Cell(1, 8).Value = "Other Location";
            ws.Cell(1, 9).Value = "Other Qty";

            int row = 2;

            foreach (var b in books)
            {
                ws.Cell(row, 1).Value = b.Title;
                ws.Cell(row, 2).Value = b.Author;
                ws.Cell(row, 3).Value = b.ISBN;
                ws.Cell(row, 4).Value = b.TotalQuantity;

                ws.Cell(row, 5).Value = b.Locations?.Almirah ?? 0;
                ws.Cell(row, 6).Value = b.Locations?.Bed ?? 0;
                ws.Cell(row, 7).Value = b.Locations?.Box ?? 0;

                ws.Cell(row, 8).Value = b.Locations?.Other?.Name ?? "";
                ws.Cell(row, 9).Value = b.Locations?.Other?.Quantity ?? 0;

                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "BookInventory.xlsx"
            );
        }
    }
}
