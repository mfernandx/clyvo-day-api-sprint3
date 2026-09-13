using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Infrastructure.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ClyvoDayApiWeb.Services
{
    public class PetService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PetService> _logger;
        private static readonly ActivitySource ActivitySource = new(TelemetryConstants.ServiceName);
        private readonly Counter<int> _petsCreatedCounter;

        public PetService(AppDbContext context, ILogger<PetService> logger, IMeterFactory meterFactory)
        {
            _context = context;
            _logger = logger;
            var meter = meterFactory.Create(TelemetryConstants.MeterName);
            _petsCreatedCounter = meter.CreateCounter<int>("pets_created_total",description: "Total de pets cadastrados");
        }

        public async Task<IEnumerable<Pet>> GetAllPetsAsync()
        {
            _logger.LogInformation("Buscando todos os pets.");

            var pets = await _context.Pets.AsNoTracking().Include(p => p.Tutor).ToListAsync();

            _logger.LogInformation("Busca de pets concluída. Total encontrado: {TotalPets}",pets.Count);

            return pets;
        }

        public async Task<Pet?> GetPetByIdAsync(int id)
        {
            _logger.LogInformation("Buscando pet com ID {PetId}.",id);

            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de buscar pet com ID inválido: {PetId}.", id);
                throw new ArgumentException("O ID do pet deve ser maior que zero.");
            }

            var pet =  await _context.Pets.AsNoTracking().Include(p => p.Tutor).FirstOrDefaultAsync(p => p.PetId == id);

            if (pet == null)
            {
                _logger.LogWarning("Pet com ID {PetId} não encontrado.",id);
            }
            else
            {
                _logger.LogInformation("Pet com ID {PetId} encontrado com sucesso.",id);
            }

            return pet;
        }

        public async Task<IEnumerable<Pet>>GetPetsByTutorIdAsync(int tutorId)
        {
            _logger.LogInformation("Buscando pets do tutor com ID {TutorId}.",tutorId);

            if (tutorId <= 0)
            {
                _logger.LogWarning("Tentativa de buscar pets com TutorId inválido: {TutorId}.",tutorId);
                throw new ArgumentException("O ID do tutor deve ser maior que zero.");

            }
                

            var tutorExists = await _context.Tutors.AnyAsync(t => t.UserId == tutorId);

            if (!tutorExists)
            {
                _logger.LogWarning("Tutor com ID {TutorId} não encontrado.",tutorId);
                throw new KeyNotFoundException("Tutor não encontrado.");
            }
                

            var pets = await _context.Pets.AsNoTracking().Where(p => p.TutorId == tutorId).ToListAsync();

            _logger.LogInformation("Foram encontrados {TotalPets} pets para o tutor {TutorId}.",pets.Count,tutorId);

            return pets;
        }

        public async Task<List<Pet>> GetByTutorIdAsync(int tutorId)
        {
            _logger.LogInformation("Buscando pets do tutor {TutorId}.",tutorId);

            var pets = await _context.Pets.AsNoTracking().Where(p => p.TutorId == tutorId).ToListAsync();

            _logger.LogInformation("Busca concluída para o tutor {TutorId}. Total: {TotalPets}.", tutorId, pets.Count);

            return pets;
        }

        public async Task<Pet> CreatePetAsync(Pet pet)
        {
            using var activity = ActivitySource.StartActivity("CreatePet");

            activity?.SetTag("pet.tutor_id",pet?.TutorId);

            _logger.LogInformation("Iniciando cadastro de pet para o tutor {TutorId}.",pet?.TutorId);

            if (pet == null)
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Dados do pet não informados.");
                _logger.LogWarning("Tentativa de cadastrar pet sem dados.");
                throw new ArgumentException("Os dados do pet são obrigatórios.");
            }
                

            if (string.IsNullOrWhiteSpace(pet.Name))
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Nome do pet não informado.");
                _logger.LogWarning("Tentativa de cadastrar pet sem nome para o tutor {TutorId}.",pet.TutorId);
                throw new ArgumentException("O nome do pet é obrigatório.");
            }
                

            if (pet.TutorId <= 0)
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Tutor inválido.");
                _logger.LogWarning("Tentativa de cadastrar pet com TutorId inválido: {TutorId}.", pet.TutorId);
                throw new ArgumentException("O tutor do pet é obrigatório.");

            }

            var tutorExists = await _context.Tutors.AnyAsync(t => t.UserId == pet.TutorId && t.IsActive);

            if (!tutorExists)
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Tutor não encontrado ou inativo.");
                _logger.LogWarning("Não foi possível cadastrar o pet {PetName}. Tutor {TutorId} não encontrado ou inativo.", pet.Name, pet.TutorId);
                throw new KeyNotFoundException("Tutor não encontrado ou inativo.");
            }

            try
            {
                _context.Pets.Add(pet);

                await _context.SaveChangesAsync();

                activity?.SetTag("pet.id",pet.PetId);

                activity?.SetTag("pet.species",pet.Species);

                activity?.SetStatus(ActivityStatusCode.Ok);

                _logger.LogInformation("Pet {PetName} cadastrado com sucesso. PetId: {PetId}, TutorId: {TutorId}.",pet.Name,pet.PetId,pet.TutorId);

                _petsCreatedCounter.Add(1,new KeyValuePair<string, object?>("species",pet.Species));

                return pet;
            }

            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error,ex.Message);

                _logger.LogError(ex,"Erro ao cadastrar o pet {PetName} para o tutor {TutorId}.",pet.Name,pet.TutorId);
                
                throw;
            }


            
        }
    }
}
