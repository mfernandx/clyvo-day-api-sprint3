using ClyvoDayApiWeb.Domain.Enums;
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
    public class DailyPetLogController : ControllerBase
    {
        private readonly DailyPetLogService _dailyPetLogService;

        public DailyPetLogController(DailyPetLogService dailyPetLogService)
        {
            _dailyPetLogService = dailyPetLogService;
        }

        
        [HttpGet]
        public async Task<ActionResult<List<DailyPetLog>>> GetAll()
        {
            var logs = await _dailyPetLogService.GetAllAsync();

            return Ok(logs);
        }

        
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DailyPetLog>> GetById(int id)
        {
            try
            {
                var log = await _dailyPetLogService.GetByIdAsync(id);

                if (log == null)
                    return NotFound("Registro diário não encontrado.");

                return Ok(log);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        
        [HttpGet("pet/{petId:int}")]
        public async Task<ActionResult<List<DailyPetLog>>> GetByPetId(int petId)
        {
            try
            {
                var logs = await _dailyPetLogService.GetByPetIdAsync(petId);

                return Ok(logs);
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

        
        [HttpPost]
        public async Task<ActionResult<DailyPetLog>> Create([FromBody] JsonElement body)
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                var petId = body.GetProperty("petId").GetInt32();
                var dailyPetLogType = body.GetProperty("dailyPetLogType").GetString();
                var content = body.GetProperty("content").GetString();
                var privacy = body.GetProperty("privacy").Deserialize<EnumPrivacy>();

                string? imageUrl = null;

                if (body.TryGetProperty("imageUrl", out var imageUrlProperty))
                {
                    imageUrl = imageUrlProperty.GetString();
                }

                var dailyPetLog = new DailyPetLog(petId, userId, dailyPetLogType ?? string.Empty, content ?? string.Empty, imageUrl, privacy);

                var created = await _dailyPetLogService.CreateAsync(dailyPetLog,userId);

                return CreatedAtAction(nameof(GetById), new { id = created.DailyPetLogId }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
        }

        //[Authorize(Roles = "Tutor")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var userId = GetAuthenticatedUserId();

                await _dailyPetLogService.DeleteAsync(id, userId);

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

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Usuário não autenticado.");

            return userId;
        }
    }
}
