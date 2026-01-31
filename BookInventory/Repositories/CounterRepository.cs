using BookInventory.Data;
using BookInventory.Models;
using DocumentFormat.OpenXml.InkML;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BookInventory.Repositories
{
    public class CounterRepository
    {
        private readonly IMongoCollection<Counter> _counters;
        private readonly IMongoCollection<BsonDocument> _enquiryCounters;

        public CounterRepository(MongoContext context)
        {
            _counters = context.Database.GetCollection<Counter>("Counters");
            _enquiryCounters = context.Database.GetCollection<BsonDocument>("EnquiryCounters");
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

        public int GetNext(string key)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", key);
            var update = Builders<BsonDocument>.Update.Inc("Value", 1);

            var result = _enquiryCounters.FindOneAndUpdate(
                filter,
                update,
                new FindOneAndUpdateOptions<BsonDocument>
                {
                    IsUpsert = true,
                    ReturnDocument = ReturnDocument.After
                });

            return result["Value"].AsInt32;
        }
    }
}
