using BookInventory.Data;
using BookInventory.Models;
using MongoDB.Driver;

namespace BookInventory.Repositories
{
    public class InvoiceRepository
    {
        private readonly IMongoCollection<Invoice> _invoices;

        public InvoiceRepository(MongoContext context)
        {
            _invoices = context.Database.GetCollection<Invoice>("Invoices");
        }

        public void Save(Invoice invoice)
        {
            _invoices.InsertOne(invoice);
        }

        public List<Invoice> GetAll()
        => _invoices.Find(_ => true)
                    .SortByDescending(x => x.InvoiceDate)
                    .ToList();

        public Invoice GetById(string id)
            => _invoices.Find(x => x.Id == id).FirstOrDefault();
    }
}
