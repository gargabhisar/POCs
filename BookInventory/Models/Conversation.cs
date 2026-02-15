using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookInventory.Models
{
    public class Conversation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string PhoneNumber { get; set; } // WhatsApp number (unique)
        public string ContactName { get; set; } // Optional – from webhook profile.name
        public string LastMessageText { get; set; } // For inbox preview
        public DateTime LastMessageAt { get; set; }
        public string LastMessageDirection { get; set; } // IN / OUT
        public string Status { get; set; } = "OPEN"; // OPEN / CLOSED (future use)
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}