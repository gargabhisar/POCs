using BookInventory.Data;
using BookInventory.Models;
using MongoDB.Driver;

namespace BookInventory.Repositories
{
    public class ConversationRepository
    {
        private readonly IMongoCollection<Conversation> _conversations;

        public ConversationRepository(MongoContext context)
        {
            _conversations = context.Database.GetCollection<Conversation>("Conversations");
        }

        // 🔑 Find or create conversation by phone number
        public async Task<Conversation> GetOrCreateAsync(string phoneNumber, string contactName = null)
        {
            var conversation =
                await _conversations
                    .Find(x => x.PhoneNumber == phoneNumber)
                    .FirstOrDefaultAsync();

            if (conversation != null)
            {
                if (string.IsNullOrWhiteSpace(conversation.ContactName) && !string.IsNullOrWhiteSpace(contactName))
                {
                    var update = Builders<Conversation>.Update
                        .Set(x => x.ContactName, contactName)
                        .Set(x => x.UpdatedAt, DateTime.UtcNow);

                    await _conversations.UpdateOneAsync(
                        x => x.Id == conversation.Id,
                        update
                    );

                    conversation.ContactName = contactName;
                }

                return conversation;
            }                

            conversation = new Conversation
            {
                PhoneNumber = phoneNumber,
                ContactName = contactName,
                Status = "OPEN",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _conversations.InsertOneAsync(conversation);

            return conversation;
        }

        // 🔄 Update last message preview
        public async Task UpdateLastMessageAsync(string conversationId, string text, string direction)
        {
            var update = Builders<Conversation>.Update
                .Set(x => x.LastMessageText, text)
                .Set(x => x.LastMessageDirection, direction)
                .Set(x => x.LastMessageAt, DateTime.UtcNow)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            await _conversations.UpdateOneAsync(
                x => x.Id == conversationId,
                update
            );
        }

        public async Task<List<Conversation>> GetAllAsync()
        {
            return await _conversations
                .Find(_ => true)
                .SortByDescending(x => x.LastMessageAt)
                .ToListAsync();
        }

        public async Task<Conversation> GetByIdAsync(string id)
        {
            return await _conversations
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();
        }
    }
}
