using ClyvoDayApiWeb.MongoDB.Documents;
using ClyvoDayApiWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MongoDB.Bson;

namespace ClyvoDayApiWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Tutor")]
    public class PetTimelineController : ControllerBase
    {
        private readonly IPetTimelineService _service;

        public PetTimelineController(IPetTimelineService service)
        {
            _service = service;
        }

        private int GetTutorId()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userId, out int tutorId))
                throw new UnauthorizedAccessException("Identificação do tutor inválida.");

            return tutorId;
        }

        private bool IsValidObjectId(string id)
        {
            return ObjectId.TryParse(id, out _);
        }

        // POST: api/PetTimeline
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PetTimelineEntry entry)
        {
            try
            {
                int tutorId = GetTutorId();

                // Dados controlados exclusivamente pelo servidor
                entry.Id = null;
                entry.CreatedByUserId = tutorId;

                var createdEntry = await _service.CreateAsync(entry, tutorId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdEntry.Id },
                    createdEntry
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/PetTimeline/pet/1
        [HttpGet("pet/{petId}")]
        public async Task<IActionResult> GetByPetId(int petId)
        {
            try
            {
                int tutorId = GetTutorId();

                var entries = await _service.GetByPetIdAsync(petId, tutorId);

                return Ok(entries);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/PetTimeline/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (!IsValidObjectId(id))
                return BadRequest(new { message = "Identificador inválido." });

            try
            {
                int tutorId = GetTutorId();

                var entry = await _service.GetByIdAsync(id, tutorId);

                if (entry == null)
                    return NotFound(new { message = "Registro não encontrado." });

                return Ok(entry);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // PUT: api/PetTimeline/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            string id,
            [FromBody] PetTimelineEntry entry)
        {
            if (!IsValidObjectId(id))
                return BadRequest(new { message = "Identificador inválido." });

            try
            {
                int tutorId = GetTutorId();

                await _service.UpdateAsync(id, entry, tutorId);

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/PetTimeline/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!IsValidObjectId(id))
                return BadRequest(new { message = "Identificador inválido." });

            try
            {
                int tutorId = GetTutorId();

                await _service.DeleteAsync(id, tutorId);

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
