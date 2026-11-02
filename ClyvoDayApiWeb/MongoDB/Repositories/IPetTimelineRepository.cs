using ClyvoDayApiWeb.MongoDB.Documents;

namespace ClyvoDayApiWeb.MongoDB.Repositories
{
    public interface IPetTimelineRepository
    {
        Task CreateAsync(PetTimelineEntry entry);
        Task<List<PetTimelineEntry>> GetByPetIdAsync(int petId);
        Task<PetTimelineEntry?> GetByIdAsync(string id);
        Task UpdateAsync(string id, PetTimelineEntry entry);
        Task DeleteAsync(string id);
    }
}
