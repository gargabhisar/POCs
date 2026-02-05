using BookInventory.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace BookInventory.Services
{
    public class InvoicePdfService
    {
        const string GSTIN = "09AAECI4545H1ZG";
        const string PAN = "AAECI4545H";

        const string AddressLine =
            "T3-706, Exotica Dreamville, Sector 16C,\n" +
            "Greater Noida West, Gautam Buddha Nagar,\n" +
            "Uttar Pradesh - 201308";

        const string Phone = "+91-8696-333-000";
        const string Website = "www.inkquills.in";
        const string Email = "info@inkquills.in";

        public byte[] GenerateInvoicePdf(Invoice invoice)
        {
            var subTotal = invoice.Items.Sum(i => i.MRP * i.Quantity);
            var totalDiscount = invoice.Items.Sum(i =>
                (i.MRP * i.DiscountPercent / 100) * i.Quantity
            );

            var logoPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "inkquills-logo.png"
            );

            var signaturePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "signature.png"
            );

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.Background(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // ❗ IMPORTANT:
                    // page.Content() is FULL HEIGHT
                    // So we add ONE wrapper Item inside it for dynamic border
                    page.Content().Column(pageCol =>
                    {
                        pageCol.Item()
                            .Border(1)
                            .BorderColor(Colors.Grey.Medium)
                            .Padding(10)
                            .Column(col =>
                            {
                                // ================= HEADER =================
                                col.Item()
                                   .Border(1)
                                   .BorderColor(Colors.Grey.Lighten2)
                                   .Background(Colors.White)
                                   .Padding(10)
                                   .Row(row =>
                                   {
                                       // LOGO
                                       row.ConstantItem(120)
                                           .AlignMiddle()
                                           .Image(logoPath)
                                           .FitWidth();

                                       // ADDRESS + CONTACT (CENTER)
                                       row.RelativeItem()
                                           .AlignMiddle()
                                           .AlignCenter()
                                           .Column(c =>
                                           {
                                               c.Item().AlignCenter()
                                                   .Text(AddressLine)
                                                   .FontSize(9)
                                                   .FontColor(Colors.Grey.Darken1);

                                               c.Item().PaddingTop(2)
                                                   .AlignCenter()
                                                   .Text($"{Phone} | {Website} | {Email}")
                                                   .FontSize(9)
                                                   .FontColor(Colors.Grey.Darken1);
                                           });

                                       // TAX INVOICE (RIGHT)
                                       row.ConstantItem(120)
                                           .AlignRight()
                                           .Column(c =>
                                           {
                                               c.Item().Text("TAX INVOICE")
                                                   .FontSize(16)
                                                   .Bold()
                                                   .FontColor(Colors.Blue.Darken2);

                                               c.Item().Text($"GSTIN: {GSTIN}")
                                                   .FontSize(9);
                                               c.Item().Text($"PAN: {PAN}")
                                                   .FontSize(9);
                                           });
                                   });

                                // ================= CUSTOMER + INVOICE =================
                                col.Item().PaddingTop(10).Row(row =>
                                {
                                    row.RelativeItem()
                                       .Border(1)
                                       .BorderColor(Colors.Grey.Lighten2)
                                       .Background(Colors.White)
                                       .Padding(10)
                                       .Column(c =>
                                       {
                                           c.Item().Text("Invoice To:\n").Bold();
                                           c.Item().Text($"Name: {invoice.CustomerName}");
                                           c.Item().Text($"Mobile No: {invoice.CustomerMobile}");
                                       });

                                    row.ConstantItem(220)
                                       .Border(1)
                                       .BorderColor(Colors.Grey.Lighten2)
                                       .Background(Colors.White)
                                       .Padding(10)
                                       .Column(c =>
                                       {
                                           c.Item().Text($"Invoice No: INV-{invoice.InvoiceNo:D4}").Bold();
                                           c.Item().Text($"Date: {invoice.InvoiceDate:dd-MM-yyyy}");
                                           c.Item().Text("\n").Bold();
                                           c.Item().Text($"Payment Mode: {invoice.PaymentMode}");
                                       });
                                });

                                // ================= ITEMS TABLE =================
                                col.Item().PaddingTop(10)
                                   .Border(1)
                                   .BorderColor(Colors.Grey.Lighten2)
                                   .Background(Colors.White)
                                   .Padding(5)
                                   .Table(table =>
                                   {
                                       table.ColumnsDefinition(columns =>
                                       {
                                           columns.ConstantColumn(30);
                                           columns.RelativeColumn(4);
                                           columns.RelativeColumn(1);
                                           columns.RelativeColumn(1);
                                           columns.RelativeColumn(1);
                                           columns.RelativeColumn(1);
                                           columns.RelativeColumn(1);
                                       });

                                       table.Header(header =>
                                       {
                                           header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("#").Bold();
                                           header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Item").Bold();
                                           header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("MRP").Bold();
                                           header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Qty").Bold();
                                           header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Disc %").Bold();
                                           header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Total").Bold();
                                           header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Remarks").Bold();
                                       });

                                       int i = 1;
                                       foreach (var item in invoice.Items)
                                       {
                                           table.Cell().Padding(5).Text(i++.ToString());
                                           table.Cell().Padding(5).Text(item.Title);
                                           table.Cell().Padding(5).Text(item.MRP.ToString("0.00"));
                                           table.Cell().Padding(5).Text(item.Quantity.ToString());
                                           table.Cell().Padding(5).Text($"{item.DiscountPercent}%");
                                           table.Cell().Padding(5).Text(item.LineTotal.ToString("0.00"));
                                           table.Cell().Padding(5).Text(item.Remark);
                                       }
                                   });

                                // ================= TOTALS + SIGNATURE =================
                                col.Item().PaddingTop(10).Row(row =>
                                {
                                    row.RelativeItem()
                                       .Border(1)
                                       .BorderColor(Colors.Grey.Lighten2)
                                       .Background(Colors.Grey.Lighten4)
                                       .Padding(10)
                                       .Column(c =>
                                       {
                                           if (File.Exists(signaturePath))
                                           {
                                               c.Item().AlignCenter()
                                                   .Width(120)
                                                   .Image(signaturePath)
                                                   .FitWidth();
                                           }

                                           c.Item().AlignCenter()
                                               .Text("Authorized Signatory")
                                               .Bold()
                                               .FontSize(10);
                                       });

                                    row.ConstantItem(220)
                                       .Border(1)
                                       .BorderColor(Colors.Grey.Lighten2)
                                       .Background(Colors.White)
                                       .Padding(10)
                                       .Column(c =>
                                       {
                                           c.Item().Text($"Sub Total: ₹ {subTotal:0.00}");
                                           c.Item().Text($"Discount: ₹ {totalDiscount:0.00}");
                                           c.Item().LineHorizontal(1);
                                           c.Item().Text($"Grand Total: ₹ {invoice.GrandTotal:0.00}")
                                               .FontSize(14)
                                               .Bold();
                                       });
                                });

                                // ================= FOOTER =================
                                col.Item().PaddingTop(15).AlignCenter().Column(c =>
                                {
                                    c.Item()
                                        .PaddingTop(8)
                                        .Border(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Padding(6)
                                        .AlignCenter()
                                        .Text("This is a computer generated invoice with a digital signature.")
                                        .FontSize(9)
                                        .Italic()
                                        .FontColor(Colors.Grey.Darken1);
                                });
                            });
                    });
                });
            }).GeneratePdf();
        }

        public byte[] GenerateBookSalesInvoicePdf(List<BookSales> bookSales)
        {
            var logoPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "inkquills-logo.png"
            );

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.Background(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Outer dynamic border (same as invoice)
                    page.Content().Column(pageCol =>
                    {
                        pageCol.Item()
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Padding(10)
                            .Column(col =>
                            {
                                // ================= HEADER =================
                                col.Item()
                                   .Border(1)
                                   .BorderColor(Colors.Grey.Lighten2)
                                   .Padding(10)
                                   .Row(row =>
                                   {
                                       // LOGO
                                       row.ConstantItem(120)
                                           .AlignMiddle()
                                           .Image(logoPath)
                                           .FitWidth();

                                       // ADDRESS + CONTACT (CENTER)
                                       row.RelativeItem()
                                           .AlignMiddle()
                                           .AlignCenter()
                                           .Column(c =>
                                           {
                                               c.Item().AlignCenter()
                                                   .Text(AddressLine)
                                                   .FontSize(9)
                                                   .FontColor(Colors.Grey.Darken1);

                                               c.Item().PaddingTop(2)
                                                   .AlignCenter()
                                                   .Text($"{Phone} | {Website} | {Email}")
                                                   .FontSize(9)
                                                   .FontColor(Colors.Grey.Darken1);
                                           });

                                       // REPORT TITLE (RIGHT)
                                       row.ConstantItem(120)
                                           .AlignRight()
                                           .Column(c =>
                                           {
                                               c.Item().Text("BOOK SALES REPORT")
                                                   .FontSize(14)
                                                   .Bold()
                                                   .FontColor(Colors.Blue.Darken2);

                                               c.Item().Text($"Generated: {DateTime.Now:dd-MM-yyyy}")
                                                   .FontSize(9);
                                           });
                                   });

                                // ================= TABLE =================
                                col.Item().PaddingTop(10)
                                   .Border(1)
                                   .BorderColor(Colors.Grey.Lighten2)
                                   .Padding(5)
                                   .Table(table =>
                                   {
                                       table.ColumnsDefinition(columns =>
                                       {
                                           columns.ConstantColumn(55);  // Invoice
                                           columns.ConstantColumn(70);  // Date
                                           columns.RelativeColumn(3);   // Book
                                           columns.ConstantColumn(40);  // Qty
                                           columns.ConstantColumn(50);  // MRP
                                           columns.ConstantColumn(60);  // Discount
                                           columns.ConstantColumn(60);  // Sold At
                                           columns.RelativeColumn(2);   // Payment
                                           columns.RelativeColumn(3);   // Remark
                                       });

                                       table.Header(h =>
                                       {
                                           h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Invoice").Bold();
                                           h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Date").Bold();
                                           h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Book").Bold();
                                           h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Qty").Bold();
                                           h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("MRP").Bold();
                                           h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Disc %").Bold();
                                           h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Sold At").Bold();
                                           h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Payment").Bold();
                                           h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Remark").Bold();
                                       });

                                       foreach (var r in bookSales)
                                       {
                                           table.Cell().Padding(5).Text(r.InvoiceNo);
                                           table.Cell().Padding(5).Text(r.InvoiceDate.ToString("dd-MM-yyyy"));
                                           table.Cell().Padding(5).Text(r.BookName);
                                           table.Cell().Padding(5).Text(r.Quantity.ToString());
                                           table.Cell().Padding(5).Text($"₹ {r.MRP}");
                                           table.Cell().Padding(5).Text($"{r.DiscountPercent}%");
                                           table.Cell().Padding(5).Text($"₹ {r.SoldAt}");
                                           table.Cell().Padding(5).Text(r.PaymentMode);
                                           table.Cell().Padding(5).Text(r.Remark ?? "");
                                       }
                                   });

                                // ================= FOOTER =================
                                col.Item().PaddingTop(15).AlignCenter().Column(c =>
                                {
                                    c.Item()
                                        .Border(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Padding(6)
                                        .AlignCenter()
                                        .Text("This is a computer generated report.")
                                        .FontSize(9)
                                        .Italic()
                                        .FontColor(Colors.Grey.Darken1);
                                });
                            });
                    });
                });
            }).GeneratePdf();
        }
    }
}