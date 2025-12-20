using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookInventory.Models
{
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }   // Plain text (as requested)
        public string Role { get; set; }       // Admin | Viewer
        public bool IsActive { get; set; }
    }
}
