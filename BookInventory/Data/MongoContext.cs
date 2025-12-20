using MongoDB.Driver;

namespace BookInventory.Data
{
    public class MongoContext
    {
        public IMongoDatabase Database { get; }

        public MongoContext(IConfiguration config)
        {
            var client = new MongoClient(
                config["MongoDB:ConnectionString"]
            );

            Database = client.GetDatabase(
                config["MongoDB:Database"]
            );
        }
    }
}
