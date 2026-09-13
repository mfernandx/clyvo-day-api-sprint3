using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace ClyvoDayApiWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PetMonitoringController : ControllerBase
    {
        private readonly PetMonitoringService _petMonitoringService;

        public PetMonitoringController(PetMonitoringService petMonitoringService)
        {
            _petMonitoringService = petMonitoringService;
        }


        /// <summary>
        /// Retorna todos os monitoramentos cadastrados.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<PetMonitoring>>> GetAll()
        {
            var monitorings = await _petMonitoringService.GetAllAsync();

            return Ok(monitorings);
        }


        /// <summary>
        /// Retorna um monitoramento pelo ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PetMonitoring>> GetById(int id)
        {
            try
            {
                var monitoring = await _petMonitoringService.GetByIdAsync(id);

                if (monitoring == null)
                {
                    return NotFound("Monitoramento não encontrado.");
                }

                return Ok(monitoring);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// Retorna todos os monitoramentos de um pet.
        /// </summary>
        [HttpGet("pet/{petId:int}")]
        public async Task<ActionResult<List<PetMonitoring>>> GetByPetId(int petId)
        {
            try
            {
                var monitorings = await _petMonitoringService.GetByPetIdAsync(petId);

                return Ok(monitorings);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }


        /// <summary>
        /// Cria um novo monitoramento para um pet.
        /// Apenas tutores podem criar monitoramentos.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PetMonitoring>> Create([FromBody] JsonElement body)
        {
            try
            {
                var userId = GetAuthenticatedUserId();

                if (!body.TryGetProperty("petId",out var petIdProperty))
                {
                    return BadRequest("O ID do pet é obrigatório.");
                }

                var petId = petIdProperty.GetInt32();


                string? mood =

                    body.TryGetProperty("mood",out var moodProperty)
                    && moodProperty.ValueKind != JsonValueKind.Null ? moodProperty.GetString() : null;


                string? energyLevel =
                    body.TryGetProperty("energyLevel",out var energyProperty)
                    && energyProperty.ValueKind != JsonValueKind.Null ? energyProperty.GetString() : null;


                string? hydrationLevel =
                    body.TryGetProperty("hydrationLevel",out var hydrationProperty)
                    && hydrationProperty.ValueKind != JsonValueKind.Null ? hydrationProperty.GetString() : null;


                string? food =
                    body.TryGetProperty("food",out var foodProperty)
                    && foodProperty.ValueKind != JsonValueKind.Null ? foodProperty.GetString() : null;


                string? sleepQuality =
                    body.TryGetProperty("sleepQuality",out var sleepProperty)
                    && sleepProperty.ValueKind != JsonValueKind.Null ? sleepProperty.GetString() : null;


                string? recentActivities =
                    body.TryGetProperty("recentActivities",out var activitiesProperty)
                    && activitiesProperty.ValueKind != JsonValueKind.Null ? activitiesProperty.GetString() : null;


                string? sociability =
                    body.TryGetProperty( "sociability",out var sociabilityProperty)
                    && sociabilityProperty.ValueKind != JsonValueKind.Null ? sociabilityProperty.GetString() : null;


                bool? tookMedication = null;

                if (body.TryGetProperty("tookMedication",out var medicationProperty)
                    && medicationProperty.ValueKind != JsonValueKind.Null)
                {
                    tookMedication = medicationProperty.GetBoolean();
                }


                decimal? weight = null;

                if (body.TryGetProperty("weight",out var weightProperty)
                    && weightProperty.ValueKind != JsonValueKind.Null)
                {
                    weight = weightProperty.GetDecimal();
                }


                string? observations =
                    body.TryGetProperty("observations",out var observationsProperty)
                    && observationsProperty.ValueKind != JsonValueKind.Null ? observationsProperty.GetString() : null;


                var monitoring = new PetMonitoring(
                        petId,
                        mood,
                        energyLevel,
                        hydrationLevel,
                        food,
                        sleepQuality,
                        recentActivities,
                        sociability,
                        tookMedication,
                        weight,
                        observations);


                var created = await _petMonitoringService.CreateAsync(monitoring,userId);


                return CreatedAtAction(nameof(GetById), new {id = created.PetMonitoringId},created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (JsonException)
            {
                return BadRequest("Algum campo foi enviado em um formato inválido.");
            }
        }


        /// <summary>
        /// Exclui um monitoramento.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var userId = GetAuthenticatedUserId();

                await _petMonitoringService.DeleteAsync(id,userId);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }


        /// <summary>
        /// Obtém o ID do usuário autenticado através do JWT.
        /// </summary>
        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim,out var userId))
            {
                throw new UnauthorizedAccessException("Usuário não autenticado.");
            }

            return userId;
        }
    }
}
