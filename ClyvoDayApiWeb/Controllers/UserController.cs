using ClyvoDayApiWeb.Domain.Enums;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ClyvoDayApiWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Lista todos os usuários cadastrados.
        /// </summary>
        /// <param name="type">
        /// Tipo de usuário opcional para filtragem.
        /// </param>
        /// <response code="200">
        /// Lista de usuários retornada com sucesso.
        /// </response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers([FromQuery] EnumTypeUser? type)
        {
            if (type.HasValue)
            {
                var filteredUsers = await _userService.GetUsersByTypeAsync(type.Value);

                return Ok(filteredUsers);
            }

            var users = await _userService.GetAllUsersAsync();

            return Ok(users);
        }

        /// <summary>
        /// Busca um usuário pelo identificador.
        /// </summary>
        /// <param name="id">Identificador do usuário.</param>
        /// <response code="200">Usuário encontrado.</response>
        /// <response code="400">ID inválido.</response>
        /// <response code="404">Usuário não encontrado.</response>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);

                if (user == null)
                    return NotFound("Usuário não encontrado.");

                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Atualiza o e-mail de um usuário.
        /// </summary>
        [HttpPut("{userId:int}/email")]
        public async Task<ActionResult> UpdateEmail(int userId,[FromBody] JsonElement body)
        {
            try
            {
                if (!body.TryGetProperty("email",out var emailProperty))
                {
                    return BadRequest("O e-mail é obrigatório.");
                }

                var email = emailProperty.GetString();

                if (string.IsNullOrWhiteSpace(email))
                    return BadRequest("O e-mail é obrigatório.");

                var user = await _userService.UpdateEmailAsync(userId, email);

                return Ok(new
                {
                    message = "E-mail atualizado com sucesso.",
                    userId = user.UserId,
                    email = user.Email,
                    updatedAt = user.UpdatedAt
                });
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
        /// Atualiza o telefone de um usuário.
        /// </summary>
        [HttpPut("{userId:int}/phone")]
        public async Task<ActionResult> UpdatePhoneNumber(int userId,[FromBody] JsonElement body)
        {
            try
            {
                if (!body.TryGetProperty("phoneNumber",out var phoneProperty))
                {
                    return BadRequest("O número de telefone é obrigatório.");
                }

                var phoneNumber = phoneProperty.GetString();

                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    return BadRequest("O número de telefone é obrigatório.");
                }

                var user = await _userService.UpdatePhoneNumberAsync(userId,phoneNumber);

                return Ok(new
                {
                    message = "Telefone atualizado com sucesso.",
                    userId = user.UserId,
                    phoneNumber = user.PhoneNumber,
                    updatedAt = user.UpdatedAt
                });
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
        /// Desativa um usuário.
        /// </summary>
        [HttpPut("{userId:int}/deactivate")]
        public async Task<ActionResult> DeactivateUser(int userId)
        {
            try
            {
                await _userService.DeactivateAsync(userId);

                return Ok(new{message = "Usuário desativado com sucesso."});
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{userId:int}")]
        public async Task<ActionResult> DeleteUser(int userId)
        {
            try
            {
                await _userService.DeleteAsync(userId);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (DbUpdateException)
            {
                return Conflict("Não foi possível excluir o usuário porque existem dados vinculados a ele.");
            }
        }
    }
}
