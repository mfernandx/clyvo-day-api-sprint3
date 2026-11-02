using ClyvoDayApiWeb.MongoDB.Documents;

namespace ClyvoDayApiWeb.Services
{
    public interface IPetTimelineService
    {
        Task<PetTimelineEntry> CreateAsync(PetTimelineEntry entry, int tutorId);
        Task<List<PetTimelineEntry>> GetByPetIdAsync(int petId, int tutorId);
        Task<PetTimelineEntry?> GetByIdAsync(string id, int tutorId);
        Task UpdateAsync(string id, PetTimelineEntry entry, int tutorId);
        Task DeleteAsync(string id, int tutorId);
    }
}