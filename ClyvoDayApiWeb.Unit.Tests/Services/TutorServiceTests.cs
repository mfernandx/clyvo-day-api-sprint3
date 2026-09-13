using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClyvoDayApiWeb.UnitTests.Services
{
    public class TutorServiceTests
    {
        private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
        private readonly Mock<ILogger<TutorService>> _loggerMock;

        public TutorServiceTests()
        {
            _passwordHasherMock =new Mock<IPasswordHasher<User>>();
            _loggerMock = new Mock<ILogger<TutorService>>();
        }

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            return new AppDbContext(options);
        }

        private TutorService CreateService(AppDbContext context)
        {
            return new TutorService(
                context,
                _passwordHasherMock.Object,
                _loggerMock.Object);
        }

        private static Tutor CreateTutor( string email = "maria@email.com")
        {
            return new Tutor(
                fullName: "Maria Silva",
                email: email,
                passwordHash: "123456",
                phoneNumber: "11999999999"
            );
        }

        [Fact]
        public async Task CreateTutorAsync_TutorNulo_DeveLancarArgumentException()
        {
            // Arrange
            await using var context = CreateContext();

            var service = CreateService(context);

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateTutorAsync(null!));

            // Assert
            Assert.Equal("Os dados do tutor são obrigatórios.",exception.Message);
        }

        [Fact]
        public async Task CreateTutorAsync_NomeVazio_DeveLancarArgumentException()
        {
            // Arrange
            await using var context = CreateContext();

            var service = CreateService(context);

            var tutor = new Tutor(
                fullName: "",
                email: "maria@email.com",
                passwordHash: "123456",
                phoneNumber: "11999999999"
            );

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateTutorAsync(tutor));

            // Assert
            Assert.Equal("O nome do tutor é obrigatório.",exception.Message);
        }

        [Fact]
        public async Task CreateTutorAsync_EmailVazio_DeveLancarArgumentException()
        {
            // Arrange
            await using var context = CreateContext();

            var service = CreateService(context);

            var tutor = new Tutor(
                fullName: "Maria Silva",
                email: "",
                passwordHash: "123456",
                phoneNumber: "11999999999"
            );

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateTutorAsync(tutor));

            // Assert
            Assert.Equal("O e-mail do tutor é obrigatório.",exception.Message);
        }

        [Fact]
        public async Task CreateTutorAsync_EmailJaExiste_DeveLancarInvalidOperationException()
        {
            // Arrange
            await using var context = CreateContext();

            var tutorExistente = CreateTutor("maria@email.com");

            context.Tutors.Add(tutorExistente);

            await context.SaveChangesAsync();

            var service = CreateService(context);

            var novoTutor = CreateTutor("maria@email.com");

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateTutorAsync(novoTutor));

            // Assert
            Assert.Equal("Já existe um usuário cadastrado com este e-mail.",exception.Message);
        }

        [Fact]
        public async Task CreateTutorAsync_DadosValidos_DeveCriarTutor()
        {
            // Arrange
            await using var context =CreateContext();

            var service = CreateService(context);

            var tutor = CreateTutor();

            var senhaDigitada = tutor.PasswordHash;

            var senhaHashGerada = "HASH_GERADO_PELO_TESTE";

            _passwordHasherMock.Setup(p => p.HashPassword(tutor,senhaDigitada)).Returns(senhaHashGerada);

            // Act
            var result = await service.CreateTutorAsync(tutor);

            // Assert
            Assert.NotNull(result);

            Assert.True(result.UserId > 0);

            Assert.Equal("Maria Silva",result.FullName);

            Assert.Equal("maria@email.com",result.Email);

            Assert.Equal(senhaHashGerada,result.PasswordHash);

            var tutorSalvo = await context.Tutors.FirstOrDefaultAsync(t => t.UserId == result.UserId);

            Assert.NotNull(tutorSalvo);

            Assert.Equal("maria@email.com",tutorSalvo.Email);

            Assert.Equal(senhaHashGerada,tutorSalvo.PasswordHash);

            _passwordHasherMock.Verify(p => p.HashPassword(tutor,senhaDigitada),Times.Once);
        }

        [Fact]
        public async Task GetTutorByIdAsync_IdInvalido_DeveLancarArgumentException()
        {
            // Arrange
            await using var context = CreateContext();

            var service = CreateService(context);

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.GetTutorByIdAsync(0));

            // Assert
            Assert.Equal("O ID do tutor deve ser maior que zero.",exception.Message);
        }

        [Fact]
        public async Task GetTutorByIdAsync_TutorNaoExiste_DeveRetornarNull()
        {
            // Arrange
            await using var context = CreateContext();

            var service = CreateService(context);

            // Act
            var result = await service.GetTutorByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTutorByIdAsync_TutorExiste_DeveRetornarTutor()
        {
            // Arrange
            await using var context = CreateContext();

            var tutor = CreateTutor();

            context.Tutors.Add(tutor);

            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.GetTutorByIdAsync(tutor.UserId);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(tutor.UserId,result.UserId);

            Assert.Equal(tutor.Email,result.Email);
        }

        [Fact]
        public async Task GetAllTutorsAsync_ExistemTutores_DeveRetornarTodos()
        {
            // Arrange
            await using var context =CreateContext();

            var tutor1 = CreateTutor("maria@email.com");

            var tutor2 = new Tutor(
                fullName: "João Souza",
                email: "joao@email.com",
                passwordHash: "123456",
                phoneNumber: "11888888888"
            );

            context.Tutors.AddRange(tutor1,tutor2);

            await context.SaveChangesAsync();

            var service =CreateService(context);

            // Act
            var result = (await service.GetAllTutorsAsync()).ToList();

            // Assert
            Assert.Equal(2,result.Count);

            Assert.Contains(result,t => t.Email == "maria@email.com");

            Assert.Contains(result, t => t.Email == "joao@email.com");
        }
    }
}
