using BookInventory.Data;
using BookInventory.Models;
using MongoDB.Driver;

namespace BookInventory.Repositories
{
    public class UserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(MongoContext context)
        {
            _users = context.Database.GetCollection<User>("Users");
        }

        public User GetByEmail(string email) =>
            _users.Find(u => u.Email == email).FirstOrDefault();

        public void Insert(User user) =>
            _users.InsertOne(user);
    }
}
