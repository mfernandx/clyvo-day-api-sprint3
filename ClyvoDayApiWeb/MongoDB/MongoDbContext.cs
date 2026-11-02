using MongoDB.Driver;

namespace ClyvoDayApiWeb.MongoDB
{
    public class MongoDbContext
    {
        public IMongoDatabase Database { get; }

        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoDb");
            var databaseName = configuration["MongoDb:Database"];

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("MongoDB connection string não configurada.");

            if (string.IsNullOrWhiteSpace(databaseName))
                throw new InvalidOperationException("Nome do banco MongoDB não configurado.");

            var client = new MongoClient(connectionString);

            Database = client.GetDatabase(databaseName);
        }
    }
}