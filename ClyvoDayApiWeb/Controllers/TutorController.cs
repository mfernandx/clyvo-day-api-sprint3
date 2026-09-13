using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ClyvoDayApiWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TutorController : ControllerBase
    {
        private readonly TutorService _tutorService;

        public TutorController(TutorService tutorService)
        {
            _tutorService = tutorService;
        }

        /// <summary>
        /// Lista todos os tutores cadastrados.
        /// </summary>
        /// <response code="200">
        /// Lista de tutores retornada com sucesso.
        /// </response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tutor>>> GetAllTutors()
        {
            var tutors = await _tutorService.GetAllTutorsAsync();

            return Ok(tutors);
        }

        /// <summary>
        /// Busca um tutor pelo identificador.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Tutor>> GetTutorById(int id)
        {
            try
            {
                var tutor = await _tutorService.GetTutorByIdAsync(id);

                if (tutor == null)
                    return NotFound("Tutor não encontrado.");

                return Ok(tutor);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        

        /// <summary>
        /// Cadastra um novo tutor.
        /// </summary>
        /// <response code="201">
        /// Tutor cadastrado com sucesso.
        /// </response>
        /// <response code="400">
        /// Dados inválidos ou usuário já cadastrado.
        /// </response>
        [HttpPost]
        public async Task<ActionResult<Tutor>> CreateTutor([FromBody] JsonElement body)
        {
            try
            {
                var fullName = body.GetProperty("fullName").GetString();
                var email = body.GetProperty("email").GetString();
                var password = body.GetProperty("password").GetString();
                var phoneNumber = body.GetProperty("phoneNumber").GetString();
                

                var tutor = new Tutor(
                    fullName ?? string.Empty,
                    email ?? string.Empty,
                    password ?? string.Empty,
                    phoneNumber ?? string.Empty
                    
                );

                var createdTutor = await _tutorService.CreateTutorAsync(tutor);

                return CreatedAtAction(
                    nameof(GetTutorById),
                    new { id = createdTutor.UserId },
                    createdTutor
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
