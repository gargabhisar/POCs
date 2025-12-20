using BookInventory.Models;
using BookInventory.Repositories;

namespace BookInventory.Services
{
    public class AuthService
    {
        private readonly UserRepository _repo;

        public AuthService(UserRepository repo)
        {
            _repo = repo;
        }

        public User Authenticate(string email, string password)
        {
            return _repo.Login(email, password);
        }
    }
}
