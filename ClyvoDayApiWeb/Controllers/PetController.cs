using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClyvoDayApiWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PetController : ControllerBase
    {
        private readonly PetService _petService;

        public PetController(PetService petService)
        {
            _petService = petService;
        }

        /// <summary>
        /// Lista todos os pets cadastrados.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pet>>> GetAllPets()
        {
            var pets = await _petService.GetAllPetsAsync();

            return Ok(pets);
        }

        /// <summary>
        /// Busca um pet pelo identificador.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Pet>> GetPetById(int id)
        {
            try
            {
                var pet = await _petService.GetPetByIdAsync(id);

                if (pet == null)
                    return NotFound("Pet não encontrado.");

                return Ok(pet);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Lista os pets de determinado tutor.
        /// </summary>
        [HttpGet("tutor/{tutorId}")]
        public async Task<ActionResult<IEnumerable<Pet>>>GetPetsByTutor(int tutorId)
        {
            try
            {
                var pets = await _petService.GetPetsByTutorIdAsync(tutorId);

                return Ok(pets);
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

        //[Authorize(Roles = "Tutor")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyPets()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdClaim, out var tutorId))
            {
                return Unauthorized();
            }

            var pets = await _petService.GetByTutorIdAsync(tutorId);

            return Ok(pets);
        }





        /// <summary>
        /// Cadastra um novo pet.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Pet>> CreatePet([FromBody] Pet pet)
        {
            try
            {
                var createdPet = await _petService.CreatePetAsync(pet);

                return CreatedAtAction(nameof(GetPetById),new { id = createdPet.PetId },createdPet);
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
