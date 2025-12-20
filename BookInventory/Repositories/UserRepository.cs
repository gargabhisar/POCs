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

        public User Login(string email, string password)
        {
            return _users.Find(x =>
                x.Email == email &&
                x.Password == password &&
                x.IsActive
            ).FirstOrDefault();
        }
    }
}
