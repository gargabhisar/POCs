using BookInventory.Models;
using BookInventory.Repositories;

namespace BookInventory.Services
{
    public class PublishingServiceService
    {
        private readonly PublishingServiceRepository _repo;

        public PublishingServiceService(PublishingServiceRepository repo)
        {
            _repo = repo;
        }

        public List<PublishingService> GetActiveServices()
        {
            return _repo.GetActive();
        }

        public PublishingService Get(string id)
        {
            return _repo.GetById(id);
        }

        public void Create(PublishingService service)
        {
            service.IsActive = true;
            _repo.Insert(service);
        }
    }
}
