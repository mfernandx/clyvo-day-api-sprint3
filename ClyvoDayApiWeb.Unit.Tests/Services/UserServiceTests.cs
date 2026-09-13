using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Enums;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClyvoDayApiWeb.Unit.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<ILogger<UserService>> _loggerMock;

        public UserServiceTests()
        {
            _loggerMock = new Mock<ILogger<UserService>>();
        }

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static Tutor CreateTutor(string email = "maria@email.com")
        {
            return new Tutor(
                "Maria Silva",
                email,
                "senha-hash",
                "11999999999");
        }

        private static Veterinarian CreateVeterinarian(string email = "carlos@email.com")
        {
            return new Veterinarian(
                "Carlos Souza",
                email,
                "senha-hash",
                "11988888888",
                "12345",
                "SP",
                "Clínica Geral");
        }

        [Fact]
        public async Task GetAllUsersAsync_SemUsuarios_DeveRetornarListaVazia()
        {
            using var context = CreateContext();
            var service = new UserService(context, _loggerMock.Object);

            var result = await service.GetAllUsersAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllUsersAsync_ComUsuarios_DeveRetornarTodos()
        {
            using var context = CreateContext();

            context.Tutors.Add(CreateTutor());
            context.Veterinarians.Add(CreateVeterinarian());
            await context.SaveChangesAsync();

            var service = new UserService(context, _loggerMock.Object);

            var result = (await service.GetAllUsersAsync()).ToList();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetUserByIdAsync_IdInvalido_DeveLancarArgumentException()
        {
            using var context = CreateContext();
            var service = new UserService(context, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.GetUserByIdAsync(0));

            Assert.Equal("O ID do usuário deve ser maior que zero.", exception.Message);
        }

        [Fact]
        public async Task GetUserByIdAsync_UsuarioNaoExiste_DeveRetornarNull()
        {
            using var context = CreateContext();
            var service = new UserService(context, _loggerMock.Object);

            var result = await service.GetUserByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByIdAsync_UsuarioExiste_DeveRetornarUsuario()
        {
            using var context = CreateContext();

            var tutor = CreateTutor();
            context.Tutors.Add(tutor);
            await context.SaveChangesAsync();

            var service = new UserService(context, _loggerMock.Object);

            var result = await service.GetUserByIdAsync(tutor.UserId);

            Assert.NotNull(result);
            Assert.Equal(tutor.UserId, result.UserId);
            Assert.Equal("Maria Silva", result.FullName);
        }

        [Fact]
        public async Task GetUsersByTypeAsync_TipoTutor_DeveRetornarSomenteTutores()
        {
            using var context = CreateContext();

            context.Tutors.Add(CreateTutor());
            context.Veterinarians.Add(CreateVeterinarian());
            await context.SaveChangesAsync();

            var service = new UserService(context, _loggerMock.Object);

            var result = (await service.GetUsersByTypeAsync(EnumTypeUser.Tutor)).ToList();

            Assert.Single(result);
            Assert.Equal(EnumTypeUser.Tutor, result[0].TypeUser);
        }

        [Fact]
        public async Task GetUsersByTypeAsync_TipoVeterinario_DeveRetornarSomenteVeterinarios()
        {
            using var context = CreateContext();

            context.Tutors.Add(CreateTutor());
            context.Veterinarians.Add(CreateVeterinarian());
            await context.SaveChangesAsync();

            var service = new UserService(context, _loggerMock.Object);

            var result = (await service.GetUsersByTypeAsync(EnumTypeUser.Veterinario)).ToList();

            Assert.Single(result);
            Assert.Equal(EnumTypeUser.Veterinario, result[0].TypeUser);
        }

        [Fact]
        public async Task UpdateEmailAsync_IdInvalido_DeveLancarArgumentException()
        {
            using var context = CreateContext();
            var service = new UserService(context, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateEmailAsync(0, "novo@email.com"));

            Assert.Equal("O ID do usuário deve ser maior que zero.", exception.Message);
        }

        [Fact]
        public async Task UpdateEmailAsync_EmailVazio_DeveLancarArgumentException()
        {
            using var context = CreateContext();
            var service = new UserService(context, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateEmailAsync(1, ""));

            Assert.Equal("O e-mail é obrigatório.", exception.Message);
        }

        [Fact]
        public async Task UpdateEmailAsync_UsuarioNaoExiste_DeveLancarInvalidOperationException()
        {
            using var context = CreateContext();
            var service = new UserService(context, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateEmailAsync(999, "novo@email.com"));

            Assert.Equal("Usuário não encontrado.", exception.Message);
        }

        [Fact]
        public async Task UpdateEmailAsync_UsuarioInativo_DeveLancarInvalidOperationException()
        {
            using var context = CreateContext();

            var tutor = CreateTutor();
            tutor.Deactivate();

            context.Tutors.Add(tutor);
            await context.SaveChangesAsync();

            var service = new UserService(context, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateEmailAsync(tutor.UserId, "novo@email.com"));

            Assert.Equal("Usuário inativo.", exception.Message);
        }

        [Fact]
        public async Task UpdateEmailAsync_EmailDuplicado_DeveLancarArgumentException()
        {
            using var context = CreateContext();

            var tutor1 = CreateTutor("maria@email.com");
            var tutor2 = CreateTutor("ana@email.com");

            context.Tutors.AddRange(tutor1, tutor2);
            await context.SaveChangesAsync();

            var service = new UserService(context, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateEmailAsync(tutor2.UserId, "maria@email.com"));

            Assert.Equal("Este e-mail já está sendo utilizado por outro usuário.", exception.Message);
        }

        [Fact]
        public async Task UpdateEmailAsync_DadosValidos_DeveAtualizarEmail()
        {
            using var context = CreateContext();

            var tutor = CreateTutor();
            context.Tutors.Add(tutor);
            await context.SaveChangesAsync();

            var service = new UserService(context, _loggerMock.Object);

            var result = await service.UpdateEmailAsync(tutor.UserId, "novo@email.com");

            Assert.Equal("novo@email.com", result.Email);
            Assert.NotNull(result.UpdatedAt);

            var saved = await context.Users.FindAsync(tutor.UserId);

            Assert.NotNull(saved);
            Assert.Equal("novo@email.com", saved.Email);
        }

        [Fact]
        public async Task UpdatePhoneNumberAsync_IdInvalido_DeveLancarArgumentException()
        {
            using var context = CreateContext();
            var service = new UserService(context, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.UpdatePhoneNumberAsync(0, "11977777777"));

            Assert.Equal("O ID do usuário deve ser maior que zero.", exception.Message);
        }

        [Fact]
        public async Task UpdatePhoneNumberAsync_TelefoneVazio_DeveLancarArgumentException()
        {
            using var context = CreateContext();
            var service = new UserService(context, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.UpdatePhoneNumberAsync(1, ""));

            Assert.Equal("O número de telefone é obrigatório.", exception.Message);
        }

        [Fact]
        public async Task UpdatePhoneNumberAsync_UsuarioNaoExiste_DeveLancarInvalidOperationException()
        {
            using var context = CreateContext();
            var service = new UserService(context, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdatePhoneNumberAsync(999, "11977777777"));

            Assert.Equal("Usuário não encontrado.", exception.Message);
        }

        [Fact]
        public async Task UpdatePhoneNumberAsync_UsuarioInativo_DeveLancarInvalidOperationException()
        {
            using var context = CreateContext();

            var tutor = CreateTutor();
            tutor.Deactivate();

            context.Tutors.Add(tutor);
            await context.SaveChangesAsync();

            var service = new UserService(context, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdatePhoneNumberAsync(tutor.UserId, "11977777777"));

            Assert.Equal("Usuário inativo.", exception.Message);
        }

        [Fact]
        public async Task UpdatePhoneNumberAsync_DadosValidos_DeveAtualizarTelefone()
        {
            using var context = CreateContext();

            var tutor = CreateTutor();
            context.Tutors.Add(tutor);
            await context.SaveChangesAsync();

            var service = new UserService(context, _loggerMock.Object);

            var result = await service.UpdatePhoneNumberAsync(tutor.UserId,"11977777777");

            Assert.Equal("11977777777", result.PhoneNumber);
            Assert.NotNull(result.UpdatedAt);

            var saved = await context.Users.FindAsync(tutor.UserId);

            Assert.NotNull(saved);
            Assert.Equal("11977777777", saved.PhoneNumber);
        }

        [Fact]
        public async Task DeactivateAsync_IdInvalido_DeveLancarArgumentException()
        {
            using var context = CreateContext();
            var service = new UserService(context, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.DeactivateAsync(0));

            Assert.Equal("O ID do usuário deve ser maior que zero.", exception.Message);
        }

        [Fact]
        public async Task DeactivateAsync_UsuarioNaoExiste_DeveLancarInvalidOperationException()
        {
            using var context = CreateContext();
            var service = new UserService(context, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeactivateAsync(999));

            Assert.Equal("Usuário não encontrado.", exception.Message);
        }

        [Fact]
        public async Task DeactivateAsync_UsuarioJaInativo_DeveLancarInvalidOperationException()
        {
            using var context = CreateContext();

            var tutor = CreateTutor();
            tutor.Deactivate();

            context.Tutors.Add(tutor);
            await context.SaveChangesAsync();

            var service = new UserService(context, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeactivateAsync(tutor.UserId));

            Assert.Equal("Este usuário já está inativo.", exception.Message);
        }

        [Fact]
        public async Task DeactivateAsync_UsuarioAtivo_DeveDesativarUsuario()
        {
            using var context = CreateContext();

            var tutor = CreateTutor();
            context.Tutors.Add(tutor);
            await context.SaveChangesAsync();

            var service = new UserService(context, _loggerMock.Object);

            await service.DeactivateAsync(tutor.UserId);

            var saved = await context.Users.FindAsync(tutor.UserId);

            Assert.NotNull(saved);
            Assert.False(saved.IsActive);
        }

        [Fact]
        public async Task DeleteAsync_IdInvalido_DeveLancarArgumentException()
        {
            using var context = CreateContext();
            var service = new UserService(context, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteAsync(0));

            Assert.Equal("O ID do usuário deve ser maior que zero.", exception.Message);
        }

        [Fact]
        public async Task DeleteAsync_UsuarioNaoExiste_DeveLancarInvalidOperationException()
        {
            using var context = CreateContext();
            var service = new UserService(context, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(999));

            Assert.Equal("Usuário não encontrado.", exception.Message);
        }

        [Fact]
        public async Task DeleteAsync_UsuarioExistente_DeveExcluirUsuario()
        {
            using var context = CreateContext();

            var tutor = CreateTutor();
            context.Tutors.Add(tutor);
            await context.SaveChangesAsync();

            var userId = tutor.UserId;

            var service = new UserService(context, _loggerMock.Object);

            await service.DeleteAsync(userId);

            var saved = await context.Users.FindAsync(userId);

            Assert.Null(saved);
        }
    }
}
