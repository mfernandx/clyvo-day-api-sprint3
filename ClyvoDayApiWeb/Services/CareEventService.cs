using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Infrastructure.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Serilog.Core;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ClyvoDayApiWeb.Services
{
    public class CareEventService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CareEventService> _logger;
        private static readonly ActivitySource ActivitySource = new(TelemetryConstants.ServiceName);
        private readonly Counter<int> _careEventsCreatedCounter;


        public CareEventService(AppDbContext context, ILogger<CareEventService> logger, IMeterFactory meterFactory)
        {
            _context = context;
            _logger = logger;
            var meter = meterFactory.Create(TelemetryConstants.MeterName);
            _careEventsCreatedCounter = meter.CreateCounter<int>("care_events_created_total",description: "Total de eventos de cuidado criados");
        }

        public async Task<IEnumerable<CareEvent>> GetAllCareEventsAsync()
        {
            _logger.LogInformation("Buscando todos os eventos de cuidado.");

            var careEvents = await _context.CareEvents.AsNoTracking().Include(ce => ce.Pet).OrderByDescending(ce => ce.EventDate).ToListAsync();

            _logger.LogInformation("Busca de eventos de cuidado concluída. Total encontrado: {TotalCareEvents}.", careEvents.Count);

            return careEvents;
        }

        public async Task<CareEvent?> GetCareEventByIdAsync(int id)
        {
            _logger.LogInformation("Buscando evento de cuidado com ID {CareEventId}.",id);

            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de buscar evento de cuidado com ID inválido: {CareEventId}.",id);
                
                throw new ArgumentException("O ID do evento deve ser maior que zero.");
            }
                

            var careEvent = await _context.CareEvents.AsNoTracking().Include(ce => ce.Pet).FirstOrDefaultAsync(ce => ce.CareEventId == id);

            if (careEvent == null)
            {
                _logger.LogWarning("Evento de cuidado com ID {CareEventId} não encontrado.",id);
            }
            else
            {
                _logger.LogInformation("Evento de cuidado com ID {CareEventId} encontrado com sucesso.",id);
            }

            return careEvent;

        }

        public async Task<IEnumerable<CareEvent>>GetCareEventsByPetIdAsync(int petId)
        {
            _logger.LogInformation("Buscando eventos de cuidado do pet com ID {PetId}.", petId);

            if (petId <= 0)
            {
                _logger.LogWarning("Tentativa de buscar eventos com PetId inválido: {PetId}.",petId);

                throw new ArgumentException("O ID do pet deve ser maior que zero.");
            }
                

            var petExists = await _context.Pets.AnyAsync(p => p.PetId == petId);

            if (!petExists)
            {
                _logger.LogWarning("Pet com ID {PetId} não encontrado ao buscar eventos de cuidado.", petId);
                
                throw new KeyNotFoundException("Pet não encontrado.");
            }
                

            var careEvents = await _context.CareEvents.AsNoTracking().Where(ce => ce.PetId == petId).OrderByDescending(ce => ce.EventDate).ToListAsync();

            _logger.LogInformation("Busca concluída para o pet {PetId}. Total de eventos encontrados: {TotalCareEvents}.", petId, careEvents.Count);

            return careEvents;
        }

        public async Task<CareEvent> CreateCareEventAsync(CareEvent careEvent)
        {
            using var activity =ActivitySource.StartActivity("CreateCareEvent");

            activity?.SetTag("pet.id",careEvent?.PetId);

            _logger.LogInformation("Iniciando cadastro de evento de cuidado para o pet {PetId}.", careEvent?.PetId);

            if (careEvent == null)
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Dados do evento não informados.");

                _logger.LogWarning("Tentativa de cadastrar evento de cuidado sem dados.");

                throw new ArgumentException("Os dados do evento são obrigatórios.");
            }
                

            if (careEvent.PetId <= 0)
            {
                activity?.SetStatus(ActivityStatusCode.Error,"PetId inválido.");

                _logger.LogWarning("Tentativa de cadastrar evento com PetId inválido: {PetId}.", careEvent.PetId);
                
                throw new ArgumentException("O pet do evento é obrigatório.");
            }
                

            var petExists = await _context.Pets.AnyAsync(p => p.PetId == careEvent.PetId);

            if (!petExists)
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Pet não encontrado.");

                _logger.LogWarning("Pet com ID {PetId} não encontrado ao cadastrar evento de cuidado.", careEvent.PetId);
                
                throw new KeyNotFoundException("Pet não encontrado.");
            }
                

            if (string.IsNullOrWhiteSpace(careEvent.Description))
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Descrição não informada.");

                _logger.LogWarning("Tentativa de cadastrar evento sem descrição para o pet {PetId}.", careEvent.PetId);
                
                throw new ArgumentException("A descrição do evento é obrigatória.");
            }

            try
            {
                _context.CareEvents.Add(careEvent);

                await _context.SaveChangesAsync();

                activity?.SetTag("care_event.id",careEvent.CareEventId);

                activity?.SetTag("care_event.type",careEvent.TypeEvent);

                activity?.SetStatus(ActivityStatusCode.Ok);

                _logger.LogInformation("Evento de cuidado cadastrado com sucesso. CareEventId: {CareEventId}, PetId: {PetId}.", careEvent.CareEventId, careEvent.PetId);

                _careEventsCreatedCounter.Add(1);

                return careEvent;
            }

            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error,ex.Message);
                _logger.LogError(ex, "Erro ao cadastrar evento de cuidado para o pet {PetId}.", careEvent.PetId);
                throw;
            }

        }

        public async Task CompleteCareEventAsync(int id)
        {
            _logger.LogInformation("Iniciando conclusão do evento de cuidado {CareEventId}.", id);

            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de concluir evento com ID inválido: {CareEventId}.", id);
                
                throw new ArgumentException("O ID do evento deve ser maior que zero.");
            }
                

            var careEvent = await _context.CareEvents.FindAsync(id);

            if (careEvent == null)
            {
                _logger.LogWarning("Evento de cuidado com ID {CareEventId} não encontrado para conclusão.", id);
                
                throw new KeyNotFoundException("Evento não encontrado.");
            }

            try
            {
                careEvent.Complete();
                await _context.SaveChangesAsync();

                _logger.LogInformation("Evento de cuidado {CareEventId} concluído com sucesso.",id);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao concluir evento de cuidado {CareEventId}.", id);
                throw;
            }

        }

        public async Task CancelCareEventAsync(int id)
        {
            _logger.LogInformation("Iniciando cancelamento do evento de cuidado {CareEventId}.",id);

            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de cancelar evento com ID inválido: {CareEventId}.",id);
                throw new ArgumentException("O ID do evento deve ser maior que zero.");
            }
                

            var careEvent = await _context.CareEvents.FindAsync(id);

            if (careEvent == null)
            {
                _logger.LogWarning("Evento de cuidado com ID {CareEventId} não encontrado para cancelamento.",id);
                throw new KeyNotFoundException("Evento não encontrado.");
            }

            try
            {
                careEvent.Cancel();
                await _context.SaveChangesAsync();

                _logger.LogInformation("Evento de cuidado {CareEventId} cancelado com sucesso.",id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Erro ao cancelar evento de cuidado {CareEventId}.",id);
                throw;
            }

        }

        public async Task DeleteCareEventAsync(int id)
        {
            _logger.LogInformation("Iniciando exclusão do evento de cuidado {CareEventId}.",id);

            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de excluir evento com ID inválido: {CareEventId}.",id);
                throw new ArgumentException("O ID do evento deve ser maior que zero.");
            }
                

            var careEvent = await _context.CareEvents.FindAsync(id);

            if (careEvent == null)
            {
                _logger.LogWarning("Evento de cuidado com ID {CareEventId} não encontrado para exclusão.",id);
                throw new KeyNotFoundException("Evento não encontrado.");
            }

            try
            {
                _context.CareEvents.Remove(careEvent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Evento de cuidado {CareEventId} excluído com sucesso.",id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Erro ao excluir evento de cuidado {CareEventId}.",id);
                throw;
            }

        }
    }
}
