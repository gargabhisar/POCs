using BookInventory.Data;
using BookInventory.Models;
using MongoDB.Driver;

namespace BookInventory.Repositories
{
    public class CounterRepository
    {
        private readonly IMongoCollection<Counter> _counters;

        public CounterRepository(MongoContext context)
        {
            _counters = context.Database.GetCollection<Counter>("Counters");
        }

        public int GetNextInvoiceNumber()
        {
            var filter = Builders<Counter>.Filter.Eq(x => x.Id, "InvoiceNo");
            var update = Builders<Counter>.Update.Inc(x => x.Value, 1);

            var options = new FindOneAndUpdateOptions<Counter>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            var counter = _counters.FindOneAndUpdate(filter, update, options);

            return counter.Value;
        }
    }
}
