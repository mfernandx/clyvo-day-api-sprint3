using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClyvoDayApiWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CareEventController : ControllerBase
    {
        private readonly CareEventService _careEventService;

        public CareEventController(CareEventService careEventService)
        {
            _careEventService = careEventService;
        }

        /// <summary>
        /// Lista todos os eventos de cuidado cadastrados.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CareEvent>>>GetAllCareEvents()
        {
            var careEvents = await _careEventService.GetAllCareEventsAsync();

            return Ok(careEvents);
        }

        /// <summary>
        /// Busca um evento pelo identificador.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CareEvent>>GetCareEventById(int id)
        {
            try
            {
                var careEvent = await _careEventService.GetCareEventByIdAsync(id);

                if (careEvent == null)
                    return NotFound("Evento não encontrado.");

                return Ok(careEvent);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Lista os eventos de cuidado de determinado pet.
        /// </summary>
        [HttpGet("pet/{petId}")]
        public async Task<ActionResult<IEnumerable<CareEvent>>>GetCareEventsByPetId(int petId)
        {
            try
            {
                var careEvents = await _careEventService.GetCareEventsByPetIdAsync(petId);

                return Ok(careEvents);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Cadastra um novo evento de cuidado.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CareEvent>>CreateCareEvent([FromBody] CareEvent careEvent)
        {
            try
            {
                var createdCareEvent = await _careEventService.CreateCareEventAsync(careEvent);

                return CreatedAtAction(nameof(GetCareEventById),new{id = createdCareEvent.CareEventId},createdCareEvent);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Marca um evento de cuidado como concluído.
        /// </summary>
        [HttpPut("{id}/complete")]
        public async Task<ActionResult> CompleteCareEvent(int id)
        {
            try
            {
                await _careEventService.CompleteCareEventAsync(id);

                return Ok("Evento concluído com sucesso.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Cancela um evento de cuidado.
        /// </summary>
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult> CancelCareEvent(int id)
        {
            try
            {
                await _careEventService.CancelCareEventAsync(id);

                return Ok("Evento cancelado com sucesso.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Remove um evento de cuidado.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCareEvent(int id)
        {
            try
            {
                await _careEventService.DeleteCareEventAsync(id);

                return Ok("Evento removido com sucesso.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
