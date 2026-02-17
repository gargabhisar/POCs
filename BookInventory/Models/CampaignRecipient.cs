using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookInventory.Models
{
    public class CampaignRecipient
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string CampaignId { get; set; }
        public string PhoneNumber { get; set; }
        public string WaMessageId { get; set; }
        public string DeliveryStatus { get; set; } // sent | delivered | read | failed
        public DateTime SentAt { get; set; }
    }
}
