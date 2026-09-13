using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ClyvoDayApiWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VeterinarianController : ControllerBase
    {
        private readonly VeterinarianService _veterinarianService;

        public VeterinarianController(VeterinarianService veterinarianService)
        {
            _veterinarianService = veterinarianService;
        }

        /// <summary>
        /// Lista todos os veterinários cadastrados.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Veterinarian>>>GetAllVeterinarians()
        {
            var veterinarians =await _veterinarianService.GetAllVeterinariansAsync();

            return Ok(veterinarians);
        }

        /// <summary>
        /// Busca um veterinário pelo identificador.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Veterinarian>>GetVeterinarianById(int id)
        {
            try
            {
                var veterinarian = await _veterinarianService.GetVeterinarianByIdAsync(id);

                if (veterinarian == null)
                    return NotFound("Veterinário não encontrado.");

                return Ok(veterinarian);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        
        /// <summary>
        /// Cadastra um novo veterinário.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Veterinarian>> CreateVeterinarian([FromBody] JsonElement body)
        {
            try
            {
                var fullName = body.GetProperty("fullName").GetString();
                var email = body.GetProperty("email").GetString();
                var password = body.GetProperty("password").GetString();
                var phoneNumber = body.GetProperty("phoneNumber").GetString();
                var crmv = body.GetProperty("crmv").GetString();
                var state = body.GetProperty("state").GetString();
                var specialty = body.GetProperty("specialty").GetString();

                var veterinarian = new Veterinarian(
                    fullName ?? string.Empty,
                    email ?? string.Empty,
                    password ?? string.Empty,
                    phoneNumber ?? string.Empty,
                    crmv ?? string.Empty,
                    state ?? string.Empty,
                    specialty ?? string.Empty
                );

                var createdVeterinarian = await _veterinarianService.CreateVeterinarianAsync(veterinarian);

                return CreatedAtAction(
                    nameof(GetVeterinarianById),
                    new { id = createdVeterinarian.UserId },
                    createdVeterinarian
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
