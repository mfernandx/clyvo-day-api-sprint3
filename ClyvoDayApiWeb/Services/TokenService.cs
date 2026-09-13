using ClyvoDayApiWeb.Domain.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ClyvoDayApiWeb.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("A chave JWT não foi configurada.");

            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("O emissor JWT não foi configurado.");

            var audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("A audiência JWT não foi configurada.");

            var expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"],out var minutes) ? minutes : 120;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.UserId.ToString()),

                new Claim(JwtRegisteredClaimNames.Email,user.Email),

                new Claim(ClaimTypes.Name,user.FullName),

                new Claim(ClaimTypes.Role,user.TypeUser.ToString()),

                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
