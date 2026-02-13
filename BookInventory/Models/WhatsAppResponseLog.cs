using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookInventory.Models
{
    public class WhatsAppResponseLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Mobile { get; set; }
        public string TemplateName { get; set; }
        public int HttpStatus { get; set; }
        public string Response { get; set; }
        public string WaMessageId { get; set; } // 🔑 Critical for webhook correlation
        public string DeliveryStatus { get; set; }  // sent, delivered, read, failed // 📦 Optional but future-ready
        public DateTime? UpdatedAt { get; set; }
        public DateTime SentAt { get; set; }
    }
}