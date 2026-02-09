using BookInventory.Data;
using BookInventory.Models;
using MongoDB.Driver;

namespace BookInventory.Repositories
{
    public class MongoLogRepository
    {
        private readonly IMongoCollection<WhatsAppResponseLog> _whatsAppResponseLogs;

        public MongoLogRepository(MongoContext context)
        {
            _whatsAppResponseLogs = context.Database.GetCollection<WhatsAppResponseLog>("WhatsAppResponseLogs");
        }

        public async Task SaveAsync(WhatsAppResponseLog log)
        {
            await _whatsAppResponseLogs.InsertOneAsync(log);
        }
    }
}
