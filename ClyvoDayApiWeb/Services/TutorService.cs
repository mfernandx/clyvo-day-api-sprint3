using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClyvoDayApiWeb.Services
{
    public class TutorService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<TutorService> _logger;


        public TutorService(AppDbContext context, IPasswordHasher<User> passwordHasher, ILogger<TutorService> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<IEnumerable<Tutor>> GetAllTutorsAsync()
        {
            _logger.LogInformation("Buscando todos os tutores.");

            var tutors = await _context.Tutors.AsNoTracking().Include(t => t.Pets).ToListAsync();

            _logger.LogInformation("Busca de tutores concluída. Total encontrado: {TotalTutors}", tutors.Count);

            return tutors;
        }

        public async Task<Tutor?> GetTutorByIdAsync(int id)
        {
            _logger.LogInformation("Buscando tutor com ID {TutorId}.", id);

            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de buscar tutor com ID inválido: {TutorId}.", id);
                throw new ArgumentException("O ID do tutor deve ser maior que zero.");
            }
                

            var tutor = await _context.Tutors.AsNoTracking().Include(t => t.Pets).FirstOrDefaultAsync(t => t.UserId == id);

            if (tutor == null)
            {
                _logger.LogWarning("Tutor com ID {TutorId} não encontrado.", id);
            }
            else
            {
                _logger.LogInformation("Tutor com ID {TutorId} encontrado com sucesso.", id);
            }

            return tutor;
        }

        public async Task<Tutor> CreateTutorAsync(Tutor tutor)
        {
            if (tutor == null)
            {
                _logger.LogWarning("Tentativa de cadastrar tutor sem dados.");
                throw new ArgumentException("Os dados do tutor são obrigatórios.");
            }

            _logger.LogInformation("Iniciando cadastro de tutor com e-mail {Email}.",tutor.Email);


            if (string.IsNullOrWhiteSpace(tutor.FullName))
            {
                _logger.LogWarning("Tentativa de cadastrar tutor sem nome.");
                throw new ArgumentException("O nome do tutor é obrigatório.");

            }
                

            if (string.IsNullOrWhiteSpace(tutor.Email))
            {
                _logger.LogWarning("Tentativa de cadastrar tutor sem e-mail.");
                throw new ArgumentException("O e-mail do tutor é obrigatório.");
            }
                
            var emailAlreadyExists = await _context.Users.AnyAsync(u => u.Email == tutor.Email);

            if (emailAlreadyExists)
            {
                _logger.LogWarning("Tentativa de cadastrar tutor com e-mail já existente: {Email}.",tutor.Email);
                throw new InvalidOperationException("Já existe um usuário cadastrado com este e-mail.");
            }

            try
            {

                var passwordHash = _passwordHasher.HashPassword(tutor,tutor.PasswordHash);

                tutor.UpdatePasswordHash(passwordHash);

                _context.Tutors.Add(tutor);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Tutor cadastrado com sucesso. UserId: {UserId}, Email: {Email}.",tutor.UserId,tutor.Email);

                return tutor;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Erro ao cadastrar tutor com e-mail {Email}.",tutor.Email);
                throw;
            }
        }
    }
}
