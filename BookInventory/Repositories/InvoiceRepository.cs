using BookInventory.Data;
using BookInventory.Models;
using MongoDB.Driver;

namespace BookInventory.Repositories
{
    public class InvoiceRepository
    {
        private readonly IMongoCollection<Invoice> _collection;

        public InvoiceRepository(MongoContext context)
        {
            _collection = context.Database.GetCollection<Invoice>("Invoices");
        }

        public void Save(Invoice invoice)
        {
            _collection.InsertOne(invoice);
        }
    }
}
