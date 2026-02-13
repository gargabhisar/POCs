using BookInventory.Models;
using BookInventory.Repositories;
using Microsoft.AspNetCore.Identity;

namespace BookInventory.Services
{
    public class AuthService
    {
        private readonly UserRepository _repo;
        private readonly PasswordHasher<User> _hasher;

        public AuthService(UserRepository repo)
        {
            _repo = repo;
            _hasher = new PasswordHasher<User>();
        }

        // ✅ Registration
        public bool Register(string name, string email, string password, string role)
        {
            if (_repo.GetByEmail(email) != null)
                return false;

            var user = new User
            {
                Name = name,
                Email = email,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            user.PasswordHash = _hasher.HashPassword(user, password);            

            _repo.Insert(user);
            return true;
        }

        // ✅ Login
        public User Authenticate(string email, string password)
        {
            var user = _repo.GetByEmail(email);

            if (user == null || !user.IsActive)
                return null;

            var result = _hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password
            );

            return result == PasswordVerificationResult.Success
                ? user
                : null;
        }
    }
}
