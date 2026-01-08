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

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Content().Column(col =>
                    {
                        // ================= HEADER =================
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Row(r =>
                            {
                                r.ConstantItem(120)
                                    .AlignMiddle()
                                    .Image("wwwroot/inkquills-logo.png")
                                    .FitWidth();
                            });

                            row.ConstantItem(200).AlignRight().Column(c =>
                            {
                                c.Item().Text("Invoice").FontSize(20).Bold();
                                c.Item().Text($"GSTIN: {GSTIN}");
                                c.Item().Text($"PAN: {PAN}");
                            });
                        });

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        // ================= CUSTOMER =================
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Invoice To:").Bold();
                                c.Item().Text("");
                                c.Item().Text($"Name: { invoice.CustomerName}");
                                c.Item().Text($"Mobile No: {invoice.CustomerMobile}");
                            });

                            row.ConstantItem(200).AlignRight().Column(c =>
                            {
                                c.Item().Text($"Invoice No: {invoice.InvoiceNo}");
                                c.Item().Text("");
                                c.Item().Text($"Date: {invoice.InvoiceDate:dd-MM-yyyy}");
                            });
                        });

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        // ================= ITEMS =================
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("No.").Bold();
                                header.Cell().Text("Items").Bold();
                                header.Cell().Text("Price").Bold();
                                header.Cell().Text("Qty").Bold();
                                header.Cell().Text("Discount").Bold();
                                header.Cell().Text("Total").Bold();
                            });

                            int i = 1;
                            foreach (var item in invoice.Items)
                            {
                                table.Cell().Text(i++.ToString());
                                table.Cell().Text(item.Title);
                                table.Cell().Text(item.MRP.ToString("0.00"));
                                table.Cell().Text(item.Quantity.ToString());
                                table.Cell().Text($"{item.DiscountPercent:0.#}%");
                                table.Cell().Text(item.LineTotal.ToString("0.00"));
                            }
                        });

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        // ================= TOTALS =================
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().PaddingTop(30).Text("Authorized Signatory").Bold();
                            });

                            row.ConstantItem(200).Column(c =>
                            {
                                c.Item().Text($"Sub Total: ₹ {subTotal:0.00}");
                                c.Item().Text($"Discount: ₹ {totalDiscount:0.00}");
                                c.Item().Text($"Total: ₹ {invoice.GrandTotal:0.00}")
                                    .FontSize(14)
                                    .Bold();
                            });
                        });

                        col.Item().PaddingTop(20).LineHorizontal(1);

                        // ================= FOOTER =================
                        col.Item().AlignCenter().Column(c =>
                        {
                            c.Item().Text(AddressLine).SemiBold();
                            c.Item().Text(Phone);
                            c.Item().Text($"{Website} | {Email}");
                        });
                    });
                });
            }).GeneratePdf();
        }
    }
}
