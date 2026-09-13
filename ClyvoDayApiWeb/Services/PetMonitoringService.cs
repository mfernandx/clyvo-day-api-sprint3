using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Constants;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Infrastructure.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ClyvoDayApiWeb.Services
{
    public class PetMonitoringService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PetMonitoringService> _logger;
        private static readonly ActivitySource ActivitySource = new(TelemetryConstants.ServiceName);
        private readonly Counter<int> _petMonitoringsCreatedCounter;


        public PetMonitoringService(AppDbContext context, ILogger<PetMonitoringService> logger, IMeterFactory meterFactory)
        {
            _context = context;
            _logger = logger;
            var meter = meterFactory.Create(TelemetryConstants.MeterName);
            _petMonitoringsCreatedCounter = meter.CreateCounter<int>("pet_monitorings_created_total",description: "Total de monitoramentos de pets criados");
        }

        public async Task<List<PetMonitoring>> GetAllAsync()
        {
            _logger.LogInformation("Buscando todos os monitoramentos de pets.");

            var petMonitorings = await _context.PetMonitorings.AsNoTracking().OrderByDescending(pm => pm.RegisteredAt).ToListAsync();
            
            _logger.LogInformation("Busca de monitoramentos de pets concluída. Total encontrado: {TotalPetMonitorings}.", petMonitorings.Count);
            
            return petMonitorings;
        }

        public async Task<PetMonitoring?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Buscando monitoramento de pet com ID {PetMonitoringId}.", id);

            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de buscar monitoramento de pet com ID inválido: {PetMonitoringId}.", id);
                throw new ArgumentException("O ID do monitoramento deve ser maior que zero.");
            }
                

            var petMonitoring = await _context.PetMonitorings.AsNoTracking().FirstOrDefaultAsync(pm => pm.PetMonitoringId == id);

            if (petMonitoring == null)
            {
                _logger.LogWarning("Monitoramento de pet com ID {PetMonitoringId} não encontrado.", id);
            }
            else
            {
                _logger.LogInformation("Monitoramento de pet com ID {PetMonitoringId} encontrado com sucesso.", id);
            }

            return petMonitoring;

        }

        public async Task<List<PetMonitoring>> GetByPetIdAsync(int petId)
        {
            _logger.LogInformation("Buscando monitoramentos do pet com ID {PetId}.", petId);

            if (petId <= 0)
            {
                _logger.LogWarning("Tentativa de buscar monitoramentos com PetId inválido: {PetId}.", petId);
                throw new ArgumentException("O ID do pet deve ser maior que zero.");
            }
                

            var petExists = await _context.Pets.AnyAsync(p => p.PetId == petId);

            if (!petExists)
            {
                _logger.LogWarning("Pet com ID {PetId} não encontrado ao buscar monitoramentos.", petId);
                throw new InvalidOperationException("Pet não encontrado.");
            }
                

            var petMonitorings = await _context.PetMonitorings.AsNoTracking().Where(pm => pm.PetId == petId).OrderByDescending(pm => pm.RegisteredAt).ToListAsync();

            _logger.LogInformation("Busca concluída para o pet {PetId}. Total de monitoramentos encontrados: {TotalPetMonitorings}.", petId, petMonitorings.Count);

            return petMonitorings;
        }

        public async Task<PetMonitoring> CreateAsync(PetMonitoring monitoring,int authenticatedUserId)
        {
            using var activity = ActivitySource.StartActivity("CreatePetMonitoring");

            activity?.SetTag("pet.id",monitoring?.PetId);

            activity?.SetTag("user.id",authenticatedUserId);

            if (monitoring == null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Dados do monitoramento não informados.");

                _logger.LogWarning("Tentativa de criar monitoramento sem dados.");

                throw new ArgumentException("Os dados do monitoramento são obrigatórios.");
            }

            _logger.LogInformation("Iniciando cadastro de monitoramento para o pet {PetId} pelo usuário {UserId}.",monitoring.PetId, authenticatedUserId);

            var pet = await _context.Pets.Include(p => p.Tutor).FirstOrDefaultAsync(p => p.PetId == monitoring.PetId);

            if (pet == null)
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Pet não encontrado.");
                _logger.LogWarning("Pet com ID {PetId} não encontrado ao criar monitoramento.",monitoring.PetId);
                throw new InvalidOperationException("Pet não encontrado.");
            }

            if (pet.TutorId != authenticatedUserId)
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Usuário não autorizado.");
                _logger.LogWarning("Usuário {UserId} tentou criar monitoramento para o pet {PetId}, pertencente ao tutor {TutorId}.",authenticatedUserId,pet.PetId,pet.TutorId);
                throw new UnauthorizedAccessException("Você não pode criar monitoramentos para este pet.");
            }
                

            var hasAtLeastOneField =
                !string.IsNullOrWhiteSpace(monitoring.Mood)
                || !string.IsNullOrWhiteSpace(monitoring.EnergyLevel)
                || !string.IsNullOrWhiteSpace(monitoring.HydrationLevel)
                || !string.IsNullOrWhiteSpace(monitoring.Food)
                || !string.IsNullOrWhiteSpace(monitoring.SleepQuality)
                || !string.IsNullOrWhiteSpace(monitoring.RecentActivities)
                || !string.IsNullOrWhiteSpace(monitoring.Sociability)
                || monitoring.TookMedication.HasValue
                || monitoring.Weight.HasValue
                || !string.IsNullOrWhiteSpace(monitoring.Observations);

            if (!hasAtLeastOneField)
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Nenhum campo de monitoramento preenchido.");
                _logger.LogWarning("Tentativa de criar monitoramento sem nenhum campo preenchido para o pet {PetId}.",monitoring.PetId);
                throw new ArgumentException("Preencha pelo menos um campo do monitoramento.");
            }
                

            if (monitoring.Weight.HasValue && monitoring.Weight.Value <= 0)
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Peso inválido.");
                _logger.LogWarning("Tentativa de registrar peso inválido para o pet {PetId}. Peso informado: {Weight}.", monitoring.PetId, monitoring.Weight.Value);
                throw new ArgumentException("O peso deve ser maior que zero.");
            }

            if (pet.Tutor == null)
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Tutor responsável não encontrado.");
                _logger.LogWarning("Tutor responsável pelo pet {PetId} não encontrado.",pet.PetId);
                throw new InvalidOperationException("Tutor responsável pelo pet não encontrado.");
            }

            try
            {
                _context.PetMonitorings.Add(monitoring);

                pet.Tutor.AddEngagementPoints(EngagementPoints.PetMonitoring);

                _logger.LogInformation("Pontos de engajamento adicionados ao tutor {TutorId} pela criação de monitoramento.",pet.Tutor.UserId);

                await _context.SaveChangesAsync();

                activity?.SetTag("pet_monitoring.id",monitoring.PetMonitoringId);

                activity?.SetTag("pet.tutor_id",pet.TutorId);

                activity?.SetStatus(ActivityStatusCode.Ok);

                _logger.LogInformation("Monitoramento criado com sucesso. PetMonitoringId: {PetMonitoringId}, PetId: {PetId}, UserId: {UserId}.",monitoring.PetMonitoringId,monitoring.PetId,authenticatedUserId);

                _petMonitoringsCreatedCounter.Add(1);

                return monitoring;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error,ex.Message);
                _logger.LogError(ex,"Erro ao criar monitoramento para o pet {PetId} pelo usuário {UserId}.",monitoring.PetId,authenticatedUserId);
                throw;
            }

        }

        public async Task DeleteAsync(int id,int authenticatedUserId)
        {
            _logger.LogInformation("Iniciando exclusão do monitoramento {PetMonitoringId} pelo usuário {UserId}.",id,authenticatedUserId);

            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de excluir monitoramento com ID inválido: {PetMonitoringId}.",id);
                throw new ArgumentException("O ID do monitoramento deve ser maior que zero.");
            }

            var monitoring = await _context.PetMonitorings.Include(pm => pm.Pet).FirstOrDefaultAsync(pm => pm.PetMonitoringId == id);

            if (monitoring == null)
            {
                _logger.LogWarning("Monitoramento {PetMonitoringId} não encontrado para exclusão.",id);
                throw new InvalidOperationException("Monitoramento não encontrado.");
            }

            if (monitoring.Pet == null || monitoring.Pet.TutorId != authenticatedUserId)
            {
                _logger.LogWarning("Usuário {UserId} tentou excluir o monitoramento {PetMonitoringId} do pet {PetId}, pertencente ao tutor {TutorId}.",authenticatedUserId,id, monitoring.Pet.PetId,monitoring.Pet.TutorId);
                throw new UnauthorizedAccessException("Você não pode excluir este monitoramento.");
            }

            try
            {
                _context.PetMonitorings.Remove(monitoring);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Monitoramento {PetMonitoringId} do pet {PetId} excluído com sucesso pelo usuário {UserId}.",id,monitoring.Pet.PetId,authenticatedUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Erro ao excluir monitoramento {PetMonitoringId} pelo usuário {UserId}.",id,authenticatedUserId);
                throw;
            }

        }
    }
}
