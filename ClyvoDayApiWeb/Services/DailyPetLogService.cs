using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Constants;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Infrastructure.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Diagnostics.Metrics;


namespace ClyvoDayApiWeb.Services
{
    public class DailyPetLogService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DailyPetLogService> _logger;
        private static readonly ActivitySource ActivitySource =new(TelemetryConstants.ServiceName);
        private readonly Counter<int> _dailyPetLogsCreatedCounter;


        public DailyPetLogService(AppDbContext context, ILogger<DailyPetLogService> logger, IMeterFactory meterFactory)
        {
            _context = context;
            _logger = logger;
            var meter = meterFactory.Create(TelemetryConstants.MeterName);
            _dailyPetLogsCreatedCounter = meter.CreateCounter<int>("daily_pet_logs_created_total",description: "Total de registros diários de pets criados");
        }

        public async Task<List<DailyPetLog>> GetAllAsync()
        {
            _logger.LogInformation("Buscando todos os registros diários de pets.");

            var dailyPetLogs = await _context.DailyPetLogs.AsNoTracking().Include(dpl => dpl.Pet).OrderByDescending(dpl => dpl.RegisteredAt).ToListAsync();

            _logger.LogInformation("Busca de registros diários de pets concluída. Total encontrado: {TotalDailyPetLogs}.", dailyPetLogs.Count);

            return dailyPetLogs;
        }

        public async Task<DailyPetLog?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Buscando registro diário de pet com ID {DailyPetLogId}.", id);

            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de buscar registro diário de pet com ID inválido: {DailyPetLogId}.", id);
                throw new ArgumentException("O ID do registro deve ser maior que zero.");
            }
                

            var dailyPetLog = await _context.DailyPetLogs.AsNoTracking().Include(dpl => dpl.Pet).FirstOrDefaultAsync(dpl => dpl.DailyPetLogId == id);

            if (dailyPetLog == null)
            {
                _logger.LogWarning("Registro diário de pet com ID {DailyPetLogId} não encontrado.", id);
            }
            else
            {
                _logger.LogInformation("Registro diário de pet com ID {DailyPetLogId} encontrado com sucesso.", id);
            }

            return dailyPetLog;
        }

        public async Task<List<DailyPetLog>> GetByPetIdAsync(int petId)
        {
            _logger.LogInformation("Buscando registro diário do pet com ID {PetId}.", petId);

            if (petId <= 0)
            {
                _logger.LogWarning("Tentativa de buscar registros diários com PetId inválido: {PetId}.", petId);
                throw new ArgumentException("O ID do pet deve ser maior que zero.");
            }
                

            var petExists = await _context.Pets.AnyAsync(p => p.PetId == petId);

            if (!petExists)
            {
                _logger.LogWarning("Pet com ID {PetId} não encontrado ao buscar registros diários.", petId);
                throw new InvalidOperationException("Pet não encontrado.");
            }
                

            var dailyPetLogs = await _context.DailyPetLogs.AsNoTracking().Where(dpl => dpl.PetId == petId).OrderByDescending(dpl => dpl.RegisteredAt).ToListAsync();

            _logger.LogInformation("Busca concluída para o pet {PetId}. Total de registros diários encontrados: {TotalDailyPetLogs}.", petId, dailyPetLogs.Count);

            return dailyPetLogs;

        }

        public async Task<DailyPetLog> CreateAsync(DailyPetLog dailyPetLog,int authenticatedUserId)
        {

            using var activity = ActivitySource.StartActivity("CreateDailyPetLog");

            activity?.SetTag("pet.id",dailyPetLog?.PetId);

            activity?.SetTag("user.id",authenticatedUserId);

            _logger.LogInformation("Iniciando cadastro de registro diário para o pet {PetId}.", dailyPetLog?.PetId);

            if (dailyPetLog == null)
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Dados do registro diário não informados.");

                _logger.LogWarning("Tentativa de criar registro diário sem dados.");

                throw new ArgumentException("Os dados do registro diário são obrigatórios.");
            }

            var pet = await _context.Pets.Include(p => p.Tutor).FirstOrDefaultAsync(p => p.PetId == dailyPetLog.PetId);

            if (pet == null)
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Pet não encontrado.");

                _logger.LogWarning("Pet com ID {PetId} não encontrado ao criar registro diário.",dailyPetLog.PetId);
                
                throw new InvalidOperationException("Pet não encontrado.");
            }
                

            if (pet.TutorId != authenticatedUserId)
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Usuário não autorizado.");
                
                _logger.LogWarning("Usuário {UserId} tentou criar registro diário para o pet {PetId}, pertencente ao tutor {TutorId}.", authenticatedUserId, pet.PetId, pet.TutorId);
                
                throw new UnauthorizedAccessException("Você não pode criar registros para este pet.");
            }
                

            if (string.IsNullOrWhiteSpace(dailyPetLog.Content))
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Conteúdo do registro não informado.");

                _logger.LogWarning("Tentativa de criar registro diário sem conteúdo para o pet {PetId}.",pet.PetId);
                
                throw new ArgumentException("O conteúdo do registro é obrigatório.");

            }
            

            if (pet.Tutor == null)
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Tutor responsável não encontrado.");

                _logger.LogWarning("Tutor responsável pelo pet {PetId} não foi encontrado.",pet.PetId);

                throw new InvalidOperationException("Tutor responsável pelo pet não encontrado.");
            }

            try
            {
                _context.DailyPetLogs.Add(dailyPetLog);

                pet.Tutor.AddEngagementPoints(EngagementPoints.DailyPetLog);

                _logger.LogInformation("Pontos de engajamento adicionados ao tutor {TutorId} pela criação de registro diário.", pet.Tutor.UserId);

                await _context.SaveChangesAsync();

                activity?.SetTag("daily_pet_log.id",dailyPetLog.DailyPetLogId);
                activity?.SetTag("pet.tutor_id",pet.TutorId);
                activity?.SetStatus(ActivityStatusCode.Ok);

                _logger.LogInformation("Registro diário criado com sucesso. DailyPetLogId: {DailyPetLogId}, PetId: {PetId}, UserId: {UserId}.", dailyPetLog.DailyPetLogId, dailyPetLog.PetId, authenticatedUserId);

                _dailyPetLogsCreatedCounter.Add(1);

                return dailyPetLog;
            }

            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error,ex.Message);

                _logger.LogError(ex, "Erro ao criar registro diário para o pet {PetId} pelo usuário {UserId}.", dailyPetLog.PetId, authenticatedUserId);
                
                throw;
            }

        }

        public async Task DeleteAsync(int id,int authenticatedUserId)
        {
            _logger.LogInformation("Iniciando exclusão do registro diário {DailyPetLogId} pelo usuário {UserId}.", id, authenticatedUserId);

            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de excluir registro diário com ID inválido: {DailyPetLogId}.",id);
                throw new ArgumentException("O ID do registro deve ser maior que zero.");
            }

            var dailyPetLog = await _context.DailyPetLogs.FirstOrDefaultAsync(dpl => dpl.DailyPetLogId == id);

            if (dailyPetLog == null)
            {
                _logger.LogWarning("Registro diário {DailyPetLogId} não encontrado para exclusão.",id);
                throw new InvalidOperationException("Registro diário não encontrado.");
            }
                

            if (dailyPetLog.CreatedByUserId != authenticatedUserId)
            {
                _logger.LogWarning("Usuário {UserId} tentou excluir o registro diário {DailyPetLogId}, criado pelo usuário {CreatedByUserId}.", authenticatedUserId, id, dailyPetLog.CreatedByUserId);
                throw new UnauthorizedAccessException("Você não pode excluir este registro.");
            }

            try
            {
                _context.DailyPetLogs.Remove(dailyPetLog);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Registro diário {DailyPetLogId} excluído com sucesso pelo usuário {UserId}.", id, authenticatedUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir registro diário {DailyPetLogId} pelo usuário {UserId}.", id, authenticatedUserId);
                throw;
            }

        }
    }
}