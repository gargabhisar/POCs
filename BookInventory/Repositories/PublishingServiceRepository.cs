using BookInventory.Data;
using BookInventory.Models;
using MongoDB.Driver;

namespace BookInventory.Repositories
{
    public class PublishingServiceRepository
    {
        private readonly IMongoCollection<PublishingService> _publishingServices;

        public PublishingServiceRepository(MongoContext context)
        {
            _publishingServices = context.Database.GetCollection<PublishingService>("PublishingServices");
        }

        public List<PublishingService> GetActive()
        {
            return _publishingServices
                .Find(x => x.IsActive)
                .SortBy(x => x.DisplayOrder)
                .ToList();
        }

        public PublishingService GetById(string id)
        {
            return _publishingServices.Find(x => x.Id == id).FirstOrDefault();
        }

        public void Insert(PublishingService service)
        {
            _publishingServices.InsertOne(service);
        }
    }
}
