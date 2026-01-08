using BookInventory.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace BookInventory.Services
{
    public class InvoicePdfService
    {
        public byte[] GenerateInvoicePdf(Invoice invoice)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    page.Content().Column(col =>
                    {
                        col.Item().Text("Invoice").FontSize(20).Bold();

                        col.Item().Text($"Invoice No: INV-{invoice.InvoiceNo:D4}").Bold();

                        col.Item().Text($"Customer: {invoice.CustomerName}");
                        col.Item().Text($"Mobile: {invoice.CustomerMobile}");
                        col.Item().Text($"Date: {invoice.InvoiceDate:dd-MM-yyyy}");

                        col.Item().LineHorizontal(1);

                        foreach (var item in invoice.Items)
                        {
                            col.Item().Text(
                                $"{item.Title} | Qty: {item.Quantity} | Price: ₹{item.FinalPrice} | Total: ₹{item.LineTotal}"
                            );
                        }

                        col.Item().LineHorizontal(1);
                        col.Item().Text($"Grand Total: ₹{invoice.GrandTotal}").Bold();
                    });
                });
            }).GeneratePdf();
        }
    }
}
