using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookInventory.Models
{
    public class Book
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }   // ✅ STRING
        public int SerialNo { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string ImageUrl { get; set; }

        public int TotalQuantity { get; set; }
        public BookLocations Locations { get; set; }

        public decimal MRP { get; set; }  
        public decimal DiscountPercent { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
