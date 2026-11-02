
using ClyvoDayApiWeb.Infrastructure.Diagnostics;
using ClyvoDayApiWeb.MongoDB.Documents;
using MongoDB.Driver;
using System.Diagnostics.Metrics;

namespace ClyvoDayApiWeb.MongoDB.Repositories
{
    public class PetTimelineRepository : IPetTimelineRepository
    {
        private readonly IMongoCollection<PetTimelineEntry> _collection;
        private readonly Counter<int> _entriesCreatedCounter;
        private readonly Counter<int> _entriesConsultedCounter;
        private readonly Counter<int> _entriesUpdatedCounter;
        private readonly Counter<int> _entriesDeletedCounter;

        public PetTimelineRepository(MongoDbContext context, IMeterFactory meterFactory)
        {
            _collection = context.Database.GetCollection<PetTimelineEntry>("PetTimelineEntries");

            var meter = meterFactory.Create(TelemetryConstants.MeterName);

            _entriesCreatedCounter = meter.CreateCounter<int>(
                "pet_timeline_entries_created_total",
                description: "Total de registros da jornada do pet criados");

            _entriesConsultedCounter = meter.CreateCounter<int>(
                "pet_timeline_entries_consulted_total",
                description: "Total de consultas de registros da jornada do pet");

            _entriesUpdatedCounter = meter.CreateCounter<int>(
                "pet_timeline_entries_updated_total",
                description: "Total de registros da jornada do pet atualizados");

            _entriesDeletedCounter = meter.CreateCounter<int>(
                "pet_timeline_entries_deleted_total",
                description: "Total de registros da jornada do pet excluídos");
        }

        public async Task CreateAsync(PetTimelineEntry entry)
        {
            await _collection.InsertOneAsync(entry);
            _entriesCreatedCounter.Add(1);
        }

        public async Task<List<PetTimelineEntry>> GetByPetIdAsync(int petId)
        {
            var entries = await _collection.Find(entry => entry.PetId == petId).SortByDescending(entry => entry.RegisteredAt).ToListAsync();

            _entriesConsultedCounter.Add(1);
            return entries;
        }

        public async Task<PetTimelineEntry?> GetByIdAsync(string id)
        {
            var entry = await _collection.Find(entry => entry.Id == id).FirstOrDefaultAsync();

            _entriesConsultedCounter.Add(1);
            return entry;
        }

        public async Task UpdateAsync(string id, PetTimelineEntry entry)
        {
            entry.Id = id;

            await _collection.ReplaceOneAsync(existing => existing.Id == id,entry);

            _entriesUpdatedCounter.Add(1);
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(entry => entry.Id == id);
            _entriesDeletedCounter.Add(1);
        }
    }
}
