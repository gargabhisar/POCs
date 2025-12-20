using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookInventory.Models
{
    public class Book
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string ImageUrl { get; set; }

        public int TotalQuantity { get; set; }
        public BookLocations Locations { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
