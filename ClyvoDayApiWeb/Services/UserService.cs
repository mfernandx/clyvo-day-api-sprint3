using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClyvoDayApiWeb.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;


        public UserService(AppDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            _logger.LogInformation("Buscando todos os usuários.");

            var users = await _context.Users.AsNoTracking().ToListAsync();

            _logger.LogInformation("Busca de usuários concluída. Total encontrado: {TotalUsers}", users.Count);

            return users;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            _logger.LogInformation("Buscando usuário com ID {UserId}.", id);

            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de buscar usuário com ID inválido: {UserId}.", id);
                throw new ArgumentException("O ID do usuário deve ser maior que zero.");
            }
                

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                _logger.LogWarning("Usuário com ID {UserId} não encontrado.", id);
            }
            else
            {
                _logger.LogInformation("Usuário com ID {UserId} encontrado com sucesso.", id);
            }

            return user;
        }

        public async Task<IEnumerable<User>> GetUsersByTypeAsync(EnumTypeUser type)
        {
            _logger.LogInformation("Buscando usuários do tipo {UserType}.",type);

            var users = await _context.Users.AsNoTracking().Where(u => u.TypeUser == type).ToListAsync();

            _logger.LogInformation("Busca de usuários do tipo {UserType} concluída. Total encontrado: {TotalUsers}.",type,users.Count);
            
            return users;
        }

        public async Task<User> UpdateEmailAsync(int userId, string email)
        {
            _logger.LogInformation("Iniciando atualização de e-mail do usuário {UserId}.",userId);

            if (userId <= 0)
            {
                _logger.LogWarning("Tentativa de atualizar e-mail com UserId inválido: {UserId}.",userId);
                throw new ArgumentException("O ID do usuário deve ser maior que zero.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Tentativa de atualizar e-mail do usuário {UserId} com valor vazio.",userId);
                throw new ArgumentException("O e-mail é obrigatório.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                _logger.LogWarning("Usuário {UserId} não encontrado para atualização de e-mail.",userId);
                throw new InvalidOperationException("Usuário não encontrado.");
            }
                

            if (!user.IsActive)
            {
                _logger.LogWarning("Tentativa de atualizar e-mail do usuário inativo {UserId}.",userId);
                throw new InvalidOperationException("Usuário inativo.");
            }
                
            var emailAlreadyExists = await _context.Users.AnyAsync(u => u.Email == email && u.UserId != userId);

            if (emailAlreadyExists)
            {
                _logger.LogWarning("Tentativa de atualizar o usuário {UserId} para um e-mail já utilizado.",userId);
                throw new ArgumentException("Este e-mail já está sendo utilizado por outro usuário.");
            }

            try
            {
                user.UpdateEmail(email);

                await _context.SaveChangesAsync();

                _logger.LogInformation("E-mail do usuário {UserId} atualizado com sucesso.",userId);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Erro ao atualizar o e-mail do usuário {UserId}.",userId);
                throw;
            }

        }


        public async Task<User> UpdatePhoneNumberAsync(int userId, string phoneNumber)
        {
            _logger.LogInformation("Iniciando atualização do telefone do usuário {UserId}.",userId);

            if (userId <= 0)
            {
                _logger.LogWarning("Tentativa de atualizar telefone com UserId inválido: {UserId}.",userId);

                throw new ArgumentException("O ID do usuário deve ser maior que zero.");
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                _logger.LogWarning("Tentativa de atualizar telefone do usuário {UserId} com valor vazio.",userId);

                throw new ArgumentException("O número de telefone é obrigatório.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                _logger.LogWarning("Usuário {UserId} não encontrado para atualização do telefone.",userId);
                throw new InvalidOperationException("Usuário não encontrado.");
            }
                

            if (!user.IsActive)
            {
                _logger.LogWarning("Tentativa de atualizar telefone do usuário inativo {UserId}.",userId);
                throw new InvalidOperationException("Usuário inativo.");
            }

            try
            { 
                user.UpdatePhoneNumber(phoneNumber);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Telefone do usuário {UserId} atualizado com sucesso.",userId);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Erro ao atualizar o telefone do usuário {UserId}.",userId);
                throw;
            }

        }

        public async Task DeactivateAsync(int userId)
        {
            _logger.LogInformation("Iniciando desativação do usuário {UserId}.",userId);

            if (userId <= 0)
            {
                _logger.LogWarning("Tentativa de desativar usuário com ID inválido: {UserId}.",userId);
                throw new ArgumentException("O ID do usuário deve ser maior que zero.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                _logger.LogWarning("Usuário {UserId} não encontrado para desativação.",userId);
                throw new InvalidOperationException("Usuário não encontrado.");
            }
                

            if (!user.IsActive)
            {
                _logger.LogWarning("Tentativa de desativar usuário {UserId} que já está inativo.",userId);
                throw new InvalidOperationException("Este usuário já está inativo.");
            }

            try {
                
                user.Deactivate();
                await _context.SaveChangesAsync();
                _logger.LogInformation("Usuário {UserId} desativado com sucesso.",userId);

            }

            catch (Exception ex)
            {
                _logger.LogError(ex,"Erro ao desativar o usuário {UserId}.",userId);
                throw;
            }
        }

        public async Task DeleteAsync(int userId)
        {
            _logger.LogInformation("Iniciando exclusão permanente do usuário {UserId}.", userId);

            if (userId <= 0)
            {
                _logger.LogWarning("Tentativa de excluir usuário com ID inválido: {UserId}.",userId);

                throw new ArgumentException("O ID do usuário deve ser maior que zero.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                _logger.LogWarning("Usuário {UserId} não encontrado para exclusão.",userId);
                throw new InvalidOperationException("Usuário não encontrado.");
            }

            try { 
                
                _context.Users.Remove(user);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuário {UserId} excluído permanentemente com sucesso.",userId);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex,"Erro ao excluir permanentemente o usuário {UserId}.",userId);
                throw;
            }
        }
    }
}