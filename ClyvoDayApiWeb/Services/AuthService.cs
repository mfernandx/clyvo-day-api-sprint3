using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClyvoDayApiWeb.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly TokenService _tokenService;

        public AuthService(AppDbContext context,IPasswordHasher<User> passwordHasher, TokenService tokenService )
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<(User User, string Token)> LoginAsync(string email,string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("O e-mail é obrigatório.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("A senha é obrigatória.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Usuário inativo.");

            var result = _passwordHasher.VerifyHashedPassword(user,user.PasswordHash,password);

            if (result == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("E-mail ou senha inválidos.");
            }

            var token = _tokenService.GenerateToken(user);


            return (user, token);
        }

        public async Task<User?> GetAuthenticatedUserAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("O ID do usuário deve ser maior que zero.");

            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
        }


        
    }
}
