using BookInventory.Data;
using BookInventory.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BookInventory.Repositories
{
    public class BookRepository
    {
        private readonly IMongoCollection<Book> _books;

        public BookRepository(MongoContext context)
        {
            _books = context.Database.GetCollection<Book>("Books");
        }

        public List<Book> GetAll()
            => _books.Find(_ => true).ToList();

        public Book GetById(string id)
            => _books.Find(x => x.Id == id).FirstOrDefault();

        public void Insert(Book book)
            => _books.InsertOne(book);

        public void Update(Book book)
        {
            var filter = Builders<Book>.Filter.Eq(x => x.Id, book.Id);
            _books.ReplaceOne(filter, book);
        }

        public List<Book> Search(string text)
        {
            var filter = Builders<Book>.Filter.Or(
                Builders<Book>.Filter.Regex(x => x.Title, new BsonRegularExpression(text, "i")),
                Builders<Book>.Filter.Regex(x => x.Author, new BsonRegularExpression(text, "i")),
                Builders<Book>.Filter.Regex(x => x.ISBN, new BsonRegularExpression(text, "i"))
            );

            return _books.Find(filter).ToList();
        }

        public int GetNextSerialNo()
        {
            var lastBook = _books
                .Find(_ => true)
                .SortByDescending(b => b.SerialNo)
                .Limit(1)
                .FirstOrDefault();

            return lastBook == null ? 1 : lastBook.SerialNo + 1;
        }
    }
}
