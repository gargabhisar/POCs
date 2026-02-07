using BookInventory.Models;
using BookInventory.Repositories;

namespace BookInventory.Services
{
    public class InvoiceService
    {
        private readonly InvoiceRepository _repo;

        public InvoiceService(InvoiceRepository repo)
        {
            _repo = repo;
        }

        public void Save(Invoice invoice)
        {
            _repo.Save(invoice);
        }

        public List<Invoice> GetAll() => _repo.GetAll();

        public Invoice Get(string id) => _repo.GetById(id);

        public List<BookSales> GetAll(string bookTitle = null)
        {
            var invoices = _repo.GetAll().OrderBy(x=>x.InvoiceNo).ToList();

            var rows = invoices
                .SelectMany(inv => inv.Items.Select(item => new BookSales
                {
                    InvoiceNo = inv.InvoiceNo,
                    InvoiceDate = inv.InvoiceDate,
                    BookName = item.Title,
                    Quantity = item.Quantity,
                    MRP = item.MRP,
                    DiscountPercent = item.DiscountPercent,
                    SoldAt = item.FinalPrice,
                    PaymentMode = inv.PaymentMode,
                    Remark = item.Remark
                }))
                .ToList();

            if (!string.IsNullOrWhiteSpace(bookTitle))
            {
                rows = rows
                    .Where(r => r.BookName == bookTitle)
                    .ToList();
            }

            return rows;
        }

        public List<string> GetBooksInInvoices()
        {
            return _repo.GetAll()
                .SelectMany(i => i.Items)
                .Select(i => i.Title)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        public List<BookSales> SortBookSales(List<BookSales> data, int sortColumnIndex, string sortDirection)
        {
            bool asc = sortDirection == "asc";

            return sortColumnIndex switch
            {
                0 => asc ? data.OrderBy(x => x.InvoiceNo).ToList()
                         : data.OrderByDescending(x => x.InvoiceNo).ToList(),

                1 => asc ? data.OrderBy(x => x.InvoiceDate).ToList()
                         : data.OrderByDescending(x => x.InvoiceDate).ToList(),

                2 => asc ? data.OrderBy(x => x.BookName).ToList()
                         : data.OrderByDescending(x => x.BookName).ToList(),

                3 => asc ? data.OrderBy(x => x.Quantity).ToList()
                         : data.OrderByDescending(x => x.Quantity).ToList(),

                4 => asc ? data.OrderBy(x => x.MRP).ToList()
                         : data.OrderByDescending(x => x.MRP).ToList(),

                5 => asc ? data.OrderBy(x => x.DiscountPercent).ToList()
                         : data.OrderByDescending(x => x.DiscountPercent).ToList(),

                6 => asc ? data.OrderBy(x => x.SoldAt).ToList()
                         : data.OrderByDescending(x => x.SoldAt).ToList(),

                7 => asc ? data.OrderBy(x => x.PaymentMode).ToList()
                         : data.OrderByDescending(x => x.PaymentMode).ToList(),

                8 => asc ? data.OrderBy(x => x.Remark).ToList()
                         : data.OrderByDescending(x => x.Remark).ToList(),

                _ => data
            };
        }
    }

}
