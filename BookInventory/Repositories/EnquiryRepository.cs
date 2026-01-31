using BookInventory.Data;
using BookInventory.Models;
using MongoDB.Driver;

namespace BookInventory.Repositories
{
    public class EnquiryRepository
    {
        private readonly IMongoCollection<Enquiry> _enquiries;

        public EnquiryRepository(MongoContext context)
        {
            _enquiries = context.Database.GetCollection<Enquiry>("Enquiries");
        }

        public void Insert(Enquiry enquiry)
        {
            _enquiries.InsertOne(enquiry);
        }

        public List<Enquiry> GetAll()
        {
            return _enquiries
                .Find(_ => true)
                .SortBy(x => x.SerialNo)
                .ToList();
        }
    }
}
