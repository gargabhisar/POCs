using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookInventory.Models
{
    public class Counter
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]   // ✅ THIS FIXES IT
        public string Id { get; set; }   // e.g. "InvoiceNo"

        public int Value { get; set; }
    }
}