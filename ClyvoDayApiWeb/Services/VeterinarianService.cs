using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClyvoDayApiWeb.Services
{
    public class VeterinarianService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<VeterinarianService> _logger;


        public VeterinarianService(AppDbContext context, IPasswordHasher<User> passwordHasher, ILogger<VeterinarianService> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<IEnumerable<Veterinarian>>GetAllVeterinariansAsync()
        {
            _logger.LogInformation("Buscando todos os veterinários.");

            var veterinarians = await _context.Veterinarians.AsNoTracking().ToListAsync();

            _logger.LogInformation("Busca de veterinários concluída. Total encontrado: {TotalVeterinarians}", veterinarians.Count);

            return veterinarians;
        }

        public async Task<Veterinarian?>GetVeterinarianByIdAsync(int id)
        {
            _logger.LogInformation("Buscando veterinário com ID {VeterinarianId}.", id);

            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de buscar veterinário com ID inválido: {VeterinarianId}.", id);
                throw new ArgumentException("O ID do veterinário deve ser maior que zero.");
            }
                

            var veterinarian = await _context.Veterinarians.AsNoTracking().FirstOrDefaultAsync(v => v.UserId == id);

            if (veterinarian == null)
            {
                _logger.LogWarning("Veterinário com ID {VeterinarianId} não encontrado.", id);
            }
            else
            {
                _logger.LogInformation("Veterinário com ID {VeterinarianId} encontrado com sucesso.", id);
            }

            return veterinarian;

        }

        public async Task<Veterinarian> CreateVeterinarianAsync(Veterinarian veterinarian)
        {
            if (veterinarian == null)
            {
                _logger.LogWarning("Tentativa de cadastrar veterinário sem dados.");
                throw new ArgumentException("Os dados do veterinário são obrigatórios.");
            }

            _logger.LogInformation("Iniciando cadastro de veterinário com e-mail {Email} e CRMV {Crmv}/{State}.",veterinarian.Email,veterinarian.Crmv,veterinarian.State);


            if (string.IsNullOrWhiteSpace(veterinarian.FullName))
            {
                _logger.LogWarning("Tentativa de cadastrar veterinário sem nome.");
                throw new ArgumentException("O nome do veterinário é obrigatório.");
            }
                

            if (string.IsNullOrWhiteSpace(veterinarian.Email))
            {
                _logger.LogWarning("Tentativa de cadastrar veterinário sem e-mail.");
                throw new ArgumentException("O e-mail do veterinário é obrigatório.");
            }
                

            if (string.IsNullOrWhiteSpace(veterinarian.Crmv))
            {
                _logger.LogWarning("Tentativa de cadastrar veterinário sem CRMV.");
                throw new ArgumentException("O CRMV é obrigatório.");
            }
                

            if (string.IsNullOrWhiteSpace(veterinarian.State))
            {
                _logger.LogWarning("Tentativa de cadastrar veterinário sem estado do CRMV.");
                throw new ArgumentException("O estado do CRMV é obrigatório.");
            }
                

            var emailAlreadyExists = await _context.Users.AnyAsync(u => u.Email == veterinarian.Email);

            if (emailAlreadyExists)
            {
                _logger.LogWarning("Tentativa de cadastrar veterinário com e-mail já existente: {Email}.",veterinarian.Email);
                throw new InvalidOperationException("Já existe um usuário cadastrado com este e-mail.");
            }
                
            var crmvAlreadyExists = await _context.Veterinarians.AnyAsync(v => v.Crmv == veterinarian.Crmv && v.State == veterinarian.State);

            if (crmvAlreadyExists)
            {
                _logger.LogWarning("Tentativa de cadastrar veterinário com CRMV já existente: {Crmv}/{State}.",veterinarian.Crmv,veterinarian.State);
                throw new InvalidOperationException("Já existe um veterinário cadastrado com este CRMV neste estado.");
            }

            try
            {

                var passwordHash = _passwordHasher.HashPassword(veterinarian, veterinarian.PasswordHash);

                veterinarian.UpdatePasswordHash(passwordHash);

                _context.Veterinarians.Add(veterinarian);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Veterinário cadastrado com sucesso. UserId: {UserId}, Email: {Email}, CRMV: {Crmv}/{State}.",veterinarian.UserId,veterinarian.Email,veterinarian.Crmv,veterinarian.State);

                return veterinarian;

            }

            catch (Exception ex)
            {
                _logger.LogError(ex,"Erro ao cadastrar veterinário com e-mail {Email} e CRMV {Crmv}/{State}.",veterinarian.Email,veterinarian.Crmv,veterinarian.State);
                throw;
            }
        }
    }
}
