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
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Realiza o login de um usuário.
        /// </summary>
        /// <param name="body">
        /// Objeto contendo e-mail e senha.
        /// </param>
        /// <response code="200">
        /// Login realizado com sucesso.
        /// </response>
        /// <response code="400">
        /// E-mail ou senha não informados.
        /// </response>
        /// <response code="401">
        /// Credenciais inválidas.
        /// </response>
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] JsonElement body)
        {
            try
            {
                var email = body.GetProperty("email").GetString();

                var password = body.GetProperty("password").GetString();

                var result = await _authService.LoginAsync(email ?? string.Empty,password ?? string.Empty);

                return Ok(new
                {
                    message = "Login realizado com sucesso.",
                    userId = result.User.UserId,
                    fullName = result.User.FullName,
                    email = result.User.Email,
                    typeUser = result.User.TypeUser.ToString(),
                    token = result.Token

                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }


        /// <summary>
        /// Retorna os dados do usuário autenticado.
        /// </summary>
        /// <response code="200">
        /// Usuário autenticado retornado com sucesso.
        /// </response>
        /// <response code="401">
        /// Token ausente, inválido ou expirado.
        /// </response>
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult> GetAuthenticatedUser()
        {
            var userIdClaim =
                User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Token inválido.");

            var user = await _authService.GetAuthenticatedUserAsync(userId);

            if (user == null)
                return NotFound("Usuário não encontrado.");

            if (user is Veterinarian veterinarian)
            {
                return Ok(new
                {
                    userId = veterinarian.UserId,
                    fullName = veterinarian.FullName,
                    email = veterinarian.Email,
                    phoneNumber = veterinarian.PhoneNumber,
                    typeUser = veterinarian.TypeUser.ToString(),
                    isActive = veterinarian.IsActive,

                    crmv = veterinarian.Crmv,
                    state = veterinarian.State,
                    specialty = veterinarian.Specialty
                });
            }

            if (user is Tutor tutor)
            {
                return Ok(new
                {
                    userId = tutor.UserId,
                    fullName = tutor.FullName,
                    email = tutor.Email,
                    phoneNumber = tutor.PhoneNumber,
                    typeUser = tutor.TypeUser.ToString(),
                    isActive = tutor.IsActive,

                    scoreEngagement = tutor.ScoreEngagement,
                    achievement = tutor.Achievement.ToString()
                });
            }

            return Ok(new
            {
                userId = user.UserId,
                fullName = user.FullName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                typeUser = user.TypeUser.ToString(),
                isActive = user.IsActive
            });
        }


    }
}
