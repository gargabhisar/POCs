using BookInventory.Data;
using BookInventory.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using MongoDB.Driver;

namespace BookInventory.Repositories
{
    public class CampaignRecipientRepository
    {
        private readonly IMongoCollection<CampaignRecipient> _recipients;

        public CampaignRecipientRepository(MongoContext context)
        {
            _recipients = context.Database.GetCollection<CampaignRecipient>("CampaignRecipients");
        }

        public async Task InsertAsync(CampaignRecipient recipient)
        {
            await _recipients.InsertOneAsync(recipient);
        }

        public async Task UpdateStatusAsync(string waMessageId, string status)
        {
            await _recipients.UpdateOneAsync(
                x => x.WaMessageId == waMessageId,
                Builders<CampaignRecipient>.Update
                    .Set(x => x.DeliveryStatus, status)
            );
        }

        public async Task<List<CampaignRecipient>> GetByCampaignAsync(string campaignId)
        {
            return await _recipients
                .Find(x => x.CampaignId == campaignId)
                .ToListAsync();
        }
    }
}
