using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookInventory.Models
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ConversationId { get; set; }
        public string Direction { get; set; } // IN = user → you, OUT = you → user
        public string MessageType { get; set; } // text / template (extend later)
        public string Text { get; set; }
        public string TemplateName { get; set; } // Only for OUT messages
        public string WaMessageId { get; set; }
        public string DeliveryStatus { get; set; } // sent / delivered / read / failed
        public DateTime Timestamp { get; set; }
    }
}
