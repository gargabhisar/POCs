using BookInventory.Data;
using BookInventory.Models;
using MongoDB.Driver;

namespace BookInventory.Services
{
    public class DashboardService
    {
        private readonly IMongoCollection<Book> _books;

        public DashboardService(MongoContext context)
        {
            _books = context.Database.GetCollection<Book>("Books");
        }

        public int TotalBooks()
            => (int)_books.CountDocuments(_ => true);

        public int TotalQuantity()
            => _books.AsQueryable().Sum(b => b.TotalQuantity);

        public int AlmirahCount()
            => _books.AsQueryable().Sum(b => b.Locations.Almirah);

        public int BedCount()
            => _books.AsQueryable().Sum(b => b.Locations.Bed);

        public int BoxCount()
            => _books.AsQueryable().Sum(b => b.Locations.Box);

        public int OtherCount()
            => _books.AsQueryable().Sum(b => b.Locations.Other.Quantity);

        public List<Book> LowStock()
            => _books.Find(b => b.TotalQuantity > 0 && b.TotalQuantity < 4).ToList();

        public List<Book> OutOfStockCount()
            => _books.Find(b => b.TotalQuantity == 0).ToList();
    }
}
