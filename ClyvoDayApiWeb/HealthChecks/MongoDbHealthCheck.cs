using ClyvoDayApiWeb.MongoDB;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;

namespace ClyvoDayApiWeb.HealthChecks
{
    public class MongoDbHealthCheck : IHealthCheck
    {
        private readonly MongoDbContext _context;

        public MongoDbHealthCheck(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.Database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);

                return HealthCheckResult.Healthy("Conexão com MongoDB estabelecida.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Não foi possível conectar ao MongoDB.", ex);
            }
        }
    }
}
