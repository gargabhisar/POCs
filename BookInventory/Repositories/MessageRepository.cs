using BookInventory.Data;
using BookInventory.Models;
using MongoDB.Driver;

namespace BookInventory.Repositories
{
    public class MessageRepository
    {
        private readonly IMongoCollection<Message> _messages;

        public MessageRepository(MongoContext context)
        {
            _messages = context.Database.GetCollection<Message>("Messages");
        }

        public async Task InsertAsync(Message message)
        {
            await _messages.InsertOneAsync(message);
        }

        public async Task UpdateStatusAsync(string waMessageId, string status)
        {
            var update = Builders<Message>.Update
                .Set(x => x.DeliveryStatus, status);

            await _messages.UpdateOneAsync(
                x => x.WaMessageId == waMessageId,
                update
            );
        }

        public async Task<List<Message>> GetByConversationAsync(string conversationId)
        {
            return await _messages
                .Find(x => x.ConversationId == conversationId)
                .SortBy(x => x.Timestamp)
                .ToListAsync();
        }
    }
}