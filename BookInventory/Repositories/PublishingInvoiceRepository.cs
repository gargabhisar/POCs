using BookInventory.Data;
using BookInventory.Models;
using MongoDB.Driver;

namespace BookInventory.Repositories
{
    public class PublishingInvoiceRepository
    {
        private readonly IMongoCollection<PublishingInvoice> _publishingInvoices;

        public PublishingInvoiceRepository(MongoContext context)
        {
            _publishingInvoices = context.Database.GetCollection<PublishingInvoice>("PublishingInvoices");
        }

        public void Insert(PublishingInvoice invoice)
        {
            _publishingInvoices.InsertOne(invoice);
        }

        public List<PublishingInvoice> GetAll()
        {
            return _publishingInvoices.Find(_ => true).ToList();
        }

        public PublishingInvoice GetById(string id)
        {
            return _publishingInvoices.Find(x => x.Id == id).FirstOrDefault();
        }
    }
}
