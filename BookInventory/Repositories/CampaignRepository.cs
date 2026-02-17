using BookInventory.Data;
using BookInventory.Models;
using MongoDB.Driver;

namespace BookInventory.Repositories
{
    public class CampaignRepository
    {
        private readonly IMongoCollection<Campaign> _campaigns;

        public CampaignRepository(MongoContext context)
        {
            _campaigns = context.Database.GetCollection<Campaign>("Campaigns");
        }

        public async Task<Campaign> CreateAsync(string name, string templateName)
        {
            var campaign = new Campaign
            {
                Name = name,
                TemplateName = templateName,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow
            };

            await _campaigns.InsertOneAsync(campaign);
            return campaign;
        }

        public async Task<List<Campaign>> GetAllAsync()
        {
            return await _campaigns
                .Find(_ => true)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Campaign> GetByIdAsync(string id)
        {
            return await _campaigns
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateStatusAsync(string id, string status)
        {
            await _campaigns.UpdateOneAsync(
                x => x.Id == id,
                Builders<Campaign>.Update.Set(x => x.Status, status)
            );
        }
    }
}
