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
        public DateTime SentAt { get; set; }
    }
}
