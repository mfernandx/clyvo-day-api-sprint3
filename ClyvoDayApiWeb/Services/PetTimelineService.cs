using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.MongoDB.Documents;
using ClyvoDayApiWeb.MongoDB.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClyvoDayApiWeb.Services
{
    public class PetTimelineService : IPetTimelineService
    {
        private readonly AppDbContext _context;
        private readonly IPetTimelineRepository _repository;

        public PetTimelineService(AppDbContext context,IPetTimelineRepository repository)
        {
            _context = context;
            _repository = repository;
        }

        private async Task<bool> PetBelongsToTutorAsync(int petId, int tutorId)
        {
            return await _context.Pets.AnyAsync(p =>p.PetId == petId && p.TutorId == tutorId);
        }

        public async Task<PetTimelineEntry> CreateAsync(PetTimelineEntry entry, int tutorId)
        {
            if (string.IsNullOrWhiteSpace(entry.Title))
                throw new ArgumentException("O título é obrigatório.");

            if (string.IsNullOrWhiteSpace(entry.Category))
                throw new ArgumentException("A categoria é obrigatória.");

            bool petBelongsToTutor = await PetBelongsToTutorAsync(entry.PetId, tutorId);

            if (!petBelongsToTutor)
                throw new KeyNotFoundException("Pet não encontrado ou não pertence ao tutor.");

            entry.CreatedByUserId = tutorId;
            entry.RegisteredAt = DateTime.UtcNow;

            await _repository.CreateAsync(entry);

            return entry;
        }

        public async Task<List<PetTimelineEntry>> GetByPetIdAsync(int petId, int tutorId)
        {
            bool petBelongsToTutor = await PetBelongsToTutorAsync(petId, tutorId);

            if (!petBelongsToTutor)
                throw new KeyNotFoundException("Pet não encontrado ou não pertence ao tutor.");

            return await _repository.GetByPetIdAsync(petId);
        }

        public async Task<PetTimelineEntry?> GetByIdAsync(string id, int tutorId)
        {
            var entry = await _repository.GetByIdAsync(id);

            if (entry == null)
                return null;

            bool petBelongsToTutor = await PetBelongsToTutorAsync(entry.PetId, tutorId);

            if (!petBelongsToTutor)
                throw new KeyNotFoundException("Registro não encontrado.");

            return entry;
        }

        public async Task UpdateAsync(string id, PetTimelineEntry entry, int tutorId)
        {
            var existingEntry = await GetByIdAsync(id, tutorId);

            if (existingEntry == null)
                throw new KeyNotFoundException("Registro da jornada não encontrado.");

            if (entry.PetId != existingEntry.PetId)
                throw new ArgumentException("Não é permitido alterar o pet do registro.");

            if (string.IsNullOrWhiteSpace(entry.Title))
                throw new ArgumentException("O título é obrigatório.");

            if (string.IsNullOrWhiteSpace(entry.Category))
                throw new ArgumentException("A categoria é obrigatória.");

            entry.Id = id;
            entry.CreatedByUserId = existingEntry.CreatedByUserId;
            entry.RegisteredAt = existingEntry.RegisteredAt;

            await _repository.UpdateAsync(id, entry);
        }

        public async Task DeleteAsync(string id, int tutorId)
        {
            var existingEntry = await GetByIdAsync(id, tutorId);

            if (existingEntry == null)
                throw new KeyNotFoundException("Registro da jornada não encontrado.");

            await _repository.DeleteAsync(id);
        }
    }
}
