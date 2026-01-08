using BookInventory.Models;
using BookInventory.Repositories;
using MongoDB.Bson;

namespace BookInventory.Services
{
    public class BookService
    {
        private readonly BookRepository _repo;

        public BookService(BookRepository repo)
        {
            _repo = repo;
        }

        public List<Book> GetAll() => _repo.GetAll();

        public List<Book> Search(string text)
            => string.IsNullOrWhiteSpace(text) ? GetAll() : _repo.Search(text);

        public Book Get(string id) => _repo.GetById(id);

        public void Save(Book book)
        {
            book.TotalQuantity = CalculateTotal(book);
            book.UpdatedAt = DateTime.Now;

            if (string.IsNullOrEmpty(book.Id))
            {
                book.CreatedAt = DateTime.Now;
                _repo.Insert(book);
            }
            else
            {
                _repo.Update(book);
            }
        }

        private int CalculateTotal(Book book)
        {
            int otherQty = book.Locations?.Other?.Quantity ?? 0;

            return book.Locations.Almirah
                 + book.Locations.Bed
                 + book.Locations.Box
                 + otherQty;
        }

        public int GetNextSerialNo()
        {
            return _repo.GetNextSerialNo();
        }
    }
}
