using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClyvoDayApiWeb.Unit.Tests.Services
{
    public class VeterinarianServiceTests
    {
        private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
        private readonly Mock<ILogger<VeterinarianService>> _loggerMock;

        public VeterinarianServiceTests()
        {
            _passwordHasherMock = new Mock<IPasswordHasher<User>>();
            _loggerMock = new Mock<ILogger<VeterinarianService>>();
        }

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetAllVeterinariansAsync_SemVeterinarios_DeveRetornarListaVazia()
        {
            using var context = CreateContext();
            var service = new VeterinarianService(context, _passwordHasherMock.Object, _loggerMock.Object);

            var result = await service.GetAllVeterinariansAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllVeterinariansAsync_ComVeterinarios_DeveRetornarTodos()
        {
            using var context = CreateContext();

            context.Veterinarians.Add(new Veterinarian(
                "Carlos Souza", "carlos@email.com", "123456",
                "11999999999", "12345", "SP", "Clínica Geral"));

            context.Veterinarians.Add(new Veterinarian(
                "Ana Lima", "ana@email.com", "123456",
                "11988888888", "54321", "SP", "Dermatologia"));

            await context.SaveChangesAsync();

            var service = new VeterinarianService(context, _passwordHasherMock.Object, _loggerMock.Object);

            var result = (await service.GetAllVeterinariansAsync()).ToList();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetVeterinarianByIdAsync_IdInvalido_DeveLancarArgumentException()
        {
            using var context = CreateContext();
            var service = new VeterinarianService(context, _passwordHasherMock.Object, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetVeterinarianByIdAsync(0));

            Assert.Equal("O ID do veterinário deve ser maior que zero.", exception.Message);
        }

        [Fact]
        public async Task GetVeterinarianByIdAsync_VeterinarioNaoExiste_DeveRetornarNull()
        {
            using var context = CreateContext();
            var service = new VeterinarianService(context, _passwordHasherMock.Object, _loggerMock.Object);

            var result = await service.GetVeterinarianByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetVeterinarianByIdAsync_VeterinarioExiste_DeveRetornarVeterinario()
        {
            using var context = CreateContext();

            var veterinarian = new Veterinarian(
                "Carlos Souza", "carlos@email.com", "123456",
                "11999999999", "12345", "SP", "Clínica Geral");

            context.Veterinarians.Add(veterinarian);
            await context.SaveChangesAsync();

            var service = new VeterinarianService(context, _passwordHasherMock.Object, _loggerMock.Object);

            var result = await service.GetVeterinarianByIdAsync(veterinarian.UserId);

            Assert.NotNull(result);
            Assert.Equal(veterinarian.UserId, result.UserId);
            Assert.Equal("Carlos Souza", result.FullName);
        }

        [Fact]
        public async Task CreateVeterinarianAsync_VeterinarioNulo_DeveLancarArgumentException()
        {
            using var context = CreateContext();
            var service = new VeterinarianService(context, _passwordHasherMock.Object, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateVeterinarianAsync(null!));

            Assert.Equal("Os dados do veterinário são obrigatórios.", exception.Message);
        }

        [Fact]
        public async Task CreateVeterinarianAsync_NomeVazio_DeveLancarArgumentException()
        {
            using var context = CreateContext();

            var veterinarian = new Veterinarian(
                "", "carlos@email.com", "123456",
                "11999999999", "12345", "SP", "Clínica Geral");

            var service = new VeterinarianService(context, _passwordHasherMock.Object, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateVeterinarianAsync(veterinarian));

            Assert.Equal("O nome do veterinário é obrigatório.", exception.Message);
        }

        [Fact]
        public async Task CreateVeterinarianAsync_EmailVazio_DeveLancarArgumentException()
        {
            using var context = CreateContext();

            var veterinarian = new Veterinarian(
                "Carlos Souza", "", "123456",
                "11999999999", "12345", "SP", "Clínica Geral");

            var service = new VeterinarianService(context, _passwordHasherMock.Object, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateVeterinarianAsync(veterinarian));

            Assert.Equal("O e-mail do veterinário é obrigatório.", exception.Message);
        }

        [Fact]
        public async Task CreateVeterinarianAsync_CrmvVazio_DeveLancarArgumentException()
        {
            using var context = CreateContext();

            var veterinarian = new Veterinarian(
                "Carlos Souza", "carlos@email.com", "123456",
                "11999999999", "", "SP", "Clínica Geral");

            var service = new VeterinarianService(context, _passwordHasherMock.Object, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateVeterinarianAsync(veterinarian));

            Assert.Equal("O CRMV é obrigatório.", exception.Message);
        }

        [Fact]
        public async Task CreateVeterinarianAsync_StateVazio_DeveLancarArgumentException()
        {
            using var context = CreateContext();

            var veterinarian = new Veterinarian(
                "Carlos Souza", "carlos@email.com", "123456",
                "11999999999", "12345", "", "Clínica Geral");

            var service = new VeterinarianService(context, _passwordHasherMock.Object, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateVeterinarianAsync(veterinarian));

            Assert.Equal("O estado do CRMV é obrigatório.", exception.Message);
        }

        [Fact]
        public async Task CreateVeterinarianAsync_EmailDuplicado_DeveLancarInvalidOperationException()
        {
            using var context = CreateContext();

            context.Veterinarians.Add(new Veterinarian(
                "Carlos Souza", "carlos@email.com", "123456",
                "11999999999", "12345", "SP", "Clínica Geral"));

            await context.SaveChangesAsync();

            var veterinarian = new Veterinarian(
                "Outro Veterinário", "carlos@email.com", "123456",
                "11988888888", "99999", "SP", "Dermatologia");

            var service = new VeterinarianService(context, _passwordHasherMock.Object, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateVeterinarianAsync(veterinarian));

            Assert.Equal("Já existe um usuário cadastrado com este e-mail.", exception.Message);
        }

        [Fact]
        public async Task CreateVeterinarianAsync_CrmvDuplicadoNoMesmoEstado_DeveLancarInvalidOperationException()
        {
            using var context = CreateContext();

            context.Veterinarians.Add(new Veterinarian(
                "Carlos Souza", "carlos@email.com", "123456",
                "11999999999", "12345", "SP", "Clínica Geral"));

            await context.SaveChangesAsync();

            var veterinarian = new Veterinarian(
                "Ana Lima", "ana@email.com", "123456",
                "11988888888", "12345", "SP", "Dermatologia");

            var service = new VeterinarianService(context, _passwordHasherMock.Object, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateVeterinarianAsync(veterinarian));

            Assert.Equal(
                "Já existe um veterinário cadastrado com este CRMV neste estado.",
                exception.Message);
        }

        [Fact]
        public async Task CreateVeterinarianAsync_MesmoCrmvEstadoDiferente_DeveCadastrarVeterinario()
        {
            using var context = CreateContext();

            context.Veterinarians.Add(new Veterinarian(
                "Carlos Souza", "carlos@email.com", "123456",
                "11999999999", "12345", "SP", "Clínica Geral"));

            await context.SaveChangesAsync();

            var veterinarian = new Veterinarian(
                "Ana Lima", "ana@email.com", "123456",
                "11988888888", "12345", "RJ", "Dermatologia");

            _passwordHasherMock
                .Setup(x => x.HashPassword(veterinarian, "123456"))
                .Returns("senha-hasheada");

            var service = new VeterinarianService(context, _passwordHasherMock.Object, _loggerMock.Object);

            var result = await service.CreateVeterinarianAsync(veterinarian);

            Assert.NotNull(result);
            Assert.True(result.UserId > 0);
            Assert.Equal("RJ", result.State);
        }

        [Fact]
        public async Task CreateVeterinarianAsync_DadosValidos_DeveCadastrarVeterinario()
        {
            using var context = CreateContext();

            var veterinarian = new Veterinarian(
                "Carlos Souza", "carlos@email.com", "123456",
                "11999999999", "12345", "SP", "Clínica Geral");

            _passwordHasherMock
                .Setup(x => x.HashPassword(veterinarian, "123456"))
                .Returns("senha-hasheada");

            var service = new VeterinarianService(context, _passwordHasherMock.Object, _loggerMock.Object);

            var result = await service.CreateVeterinarianAsync(veterinarian);

            Assert.NotNull(result);
            Assert.True(result.UserId > 0);
            Assert.Equal("senha-hasheada", result.PasswordHash);

            var saved = await context.Veterinarians.FindAsync(result.UserId);

            Assert.NotNull(saved);
            Assert.Equal("12345", saved.Crmv);
            Assert.Equal("SP", saved.State);

            _passwordHasherMock.Verify(
                x => x.HashPassword(veterinarian, "123456"),
                Times.Once);
        }
    }
}
