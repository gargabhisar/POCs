using BookInventory.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;

namespace BookInventory.Services
{
    public class PublishingInvoicePdfService
    {
        const string GSTIN = "09AAECI4545H1ZG";
        const string PAN = "AAECI4545H";

        const string BankDetails =
            @"Name: InkQuills Publishing House
            Bank Name: ICICI Bank
            Branch Name: Hinjewadi Branch
            IFSC: ICIC0000986
            Account Number: 098605007411
            Account Type: Current Account
            UPI ID: inkquillsph@icici";

        const string AddressLine =
            "T3-706, Exotica Dreamville, Sector 16C,\n" +
            "Greater Noida West, Gautam Buddha Nagar,\n" +
            "Uttar Pradesh - 201308";

        const string Phone = "+91-8696-333-000";
        const string Website = "www.inkquills.in";
        const string Email = "info@inkquills.in";

        public byte[] Generate(PublishingInvoice invoice)
        {
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

            var upiId = "inkquillsph@icici";
            var payeeName = "InkQuills Publishing House";

            // Optional: include invoice number & amount
            var upiAmount = invoice.GrandTotal.ToString("0.00");

            var upiString =
                $"upi://pay?pa={upiId}" +
                $"&pn={Uri.EscapeDataString(payeeName)}" +
                $"&am={upiAmount}" +
                $"&cu=INR";

            byte[] upiQrBytes;

            using (var qrGenerator = new QRCodeGenerator())
            using (var qrData = qrGenerator.CreateQrCode(upiString, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new PngByteQRCode(qrData))
            {
                upiQrBytes = qrCode.GetGraphic(20);
            }

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.Background(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // IMPORTANT:
                    // page.Content() is full-height
                    // so we add ONE wrapper Item inside it for dynamic border
                    page.Content().Column(pageCol =>
                    {
                        // 🔹 OUTER BORDER (CONTENT HEIGHT ONLY)
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

                                               c.Item().Text($"GSTIN: {GSTIN}").FontSize(9);
                                               c.Item().Text($"PAN: {PAN}").FontSize(9);
                                           });
                                   });

                                // ================= AUTHOR + INVOICE =================
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
                                           c.Item().Text($"Author Name: {invoice.AuthorName}");
                                           c.Item().Text($"Mobile No: {invoice.AuthorMobile}");
                                           c.Item().Text("Address:");
                                           c.Item().Text(invoice.AuthorAddress);
                                       });

                                    row.ConstantItem(220)
                                       .Border(1)
                                       .BorderColor(Colors.Grey.Lighten2)
                                       .Background(Colors.White)
                                       .Padding(10)
                                       .Column(c =>
                                       {
                                           c.Item().Text($"Invoice No: INV-{invoice.InvoiceNo:D4}")
                                               .Bold();
                                           c.Item().Text($"Date: {invoice.InvoiceDate:dd-MM-yyyy}");
                                       });
                                });

                                // ================= SERVICES TABLE =================
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
                                       });

                                       table.Header(header =>
                                       {
                                           header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("#").Bold();
                                           header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Service").Bold();
                                           header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Price").Bold();
                                           header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Disc %").Bold();
                                           header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Taxable").Bold();
                                       });

                                       int i = 1;
                                       foreach (var s in invoice.Services)
                                       {
                                           table.Cell().Padding(5).Text(i++.ToString());
                                           table.Cell().Padding(5).Text(s.ServiceName);
                                           table.Cell().Padding(5).Text(s.BasePrice.ToString("0.00"));
                                           table.Cell().Padding(5).Text($"{s.DiscountPercent}%");
                                           table.Cell().Padding(5).Text(s.TaxableAmount.ToString("0.00"));
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
                                           c.Item().Text($"Sub Total: ₹ {invoice.SubTotal:0.00}");

                                           if (invoice.TaxType == "IGST")
                                           {
                                               c.Item().Text($"IGST (18%): ₹ {invoice.IGST:0.00}");
                                           }
                                           else
                                           {
                                               c.Item().Text($"CGST (9%): ₹ {invoice.CGST:0.00}");
                                               c.Item().Text($"SGST (9%): ₹ {invoice.SGST:0.00}");
                                           }

                                           c.Item().LineHorizontal(1);

                                           c.Item().Text($"Grand Total: ₹ {invoice.GrandTotal:0.00}")
                                               .FontSize(14)
                                               .Bold();
                                       });
                                });

                                // ================= BANK DETAILS =================
                                col.Item().PaddingTop(10)
                                   .Border(1)
                                   .BorderColor(Colors.Grey.Lighten2)
                                   .Background(Colors.White)
                                   .Padding(10)
                                   .Row(row =>
                                   {
                                       // LEFT: BANK DETAILS TEXT
                                       row.RelativeItem().Column(c =>
                                       {
                                           c.Item().Text("Bank Details").Bold();

                                           c.Item().PaddingTop(5)
                                               .Text(BankDetails)
                                               .FontSize(10);

                                           c.Item().PaddingTop(6)
                                               .Text("Scan the QR code to pay via UPI.")
                                               .FontSize(9)
                                               .Italic()
                                               .FontColor(Colors.Grey.Darken1);
                                       });

                                       // RIGHT: UPI QR CODE
                                       row.ConstantItem(120)
                                           .AlignMiddle()
                                           .AlignCenter()
                                           .Image(upiQrBytes)
                                           .FitWidth();
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
    }
}