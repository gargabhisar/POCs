using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookInventory.Models
{
    public class PublishingService
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }     // Bronze, Silver, etc.
        public decimal Price { get; set; }   // Base price
        public int DisplayOrder { get; set; }   // Lower number = higher priority
        public bool IsActive { get; set; }   // Enable / Disable
    }

    public class PublishingInvoiceItem
    {
        public string ServiceId { get; set; }      // From PublishingServices
        public string ServiceName { get; set; }    // Snapshot for history
        public decimal BasePrice { get; set; }     // Snapshot price

        public int DiscountPercent { get; set; }   // 0–100 (integer)
        public decimal DiscountAmount { get; set; }

        public decimal TaxableAmount { get; set; } // After discount
        public decimal LineTotal { get; set; }     // After tax
    }
}
