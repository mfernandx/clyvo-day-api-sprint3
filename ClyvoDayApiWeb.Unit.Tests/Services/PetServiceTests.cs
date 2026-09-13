using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.Metrics;

namespace ClyvoDayApiWeb.Unit.Tests.Services
{
    public class PetServiceTests
    {
        private readonly Mock<ILogger<PetService>> _loggerMock;

        public PetServiceTests()
        {
            _loggerMock = new Mock<ILogger<PetService>>();
        }

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            return new AppDbContext(options);
        }

        private static IMeterFactory CreateMeterFactory()
        {
            var services = new ServiceCollection();

            services.AddMetrics();

            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider.GetRequiredService<IMeterFactory>();
        }

        [Fact]
        public async Task CreatePetAsync_PetNulo_DeveLancarArgumentException()
        {
            // Arrange
            await using var context = CreateContext();

            var service = new PetService(
                context,
                _loggerMock.Object,
                CreateMeterFactory());

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreatePetAsync(null!));

            // Assert
            Assert.Equal("Os dados do pet são obrigatórios.",exception.Message);
        }

        [Fact]
        public async Task CreatePetAsync_NomeVazio_DeveLancarArgumentException()
        {
            // Arrange
            await using var context =
                CreateContext();

            var service = new PetService(
                context,
                _loggerMock.Object,
                CreateMeterFactory());

            var pet = new Pet(
                tutorId: 1,
                name: "",
                species: "Gato",
                breed: "SRD",
                sex: "Fêmea",
                age: 4,
                birthDate: new DateTime(2022, 5, 10)
            );

            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => service.CreatePetAsync(pet));

            // Assert
            Assert.Equal(
                "O nome do pet é obrigatório.",
                exception.Message);
        }

        [Fact]
        public async Task CreatePetAsync_TutorIdInvalido_DeveLancarArgumentException()
        {
            // Arrange
            await using var context = CreateContext();

            var service = new PetService(
                context,
                _loggerMock.Object,
                CreateMeterFactory());

            var pet = new Pet(
                tutorId: 0,
                name: "Luna",
                species: "Gato",
                breed: "SRD",
                sex: "Fêmea",
                age: 4,
                birthDate: new DateTime(2022, 5, 10)
            );

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreatePetAsync(pet));

            // Assert
            Assert.Equal("O tutor do pet é obrigatório.",exception.Message);
        }

        [Fact]
        public async Task CreatePetAsync_TutorNaoExiste_DeveLancarKeyNotFoundException()
        {
            // Arrange
            await using var context = CreateContext();

            var service = new PetService(
                context,
                _loggerMock.Object,
                CreateMeterFactory());

            var pet = new Pet(
                tutorId: 999,
                name: "Luna",
                species: "Gato",
                breed: "SRD",
                sex: "Fêmea",
                age: 4,
                birthDate: new DateTime(2022, 5, 10)
            );

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreatePetAsync(pet));

            // Assert
            Assert.Equal("Tutor não encontrado ou inativo.",exception.Message);
        }

        [Fact]
        public async Task CreatePetAsync_DadosValidos_DeveCriarPet()
        {
            // Arrange
            await using var context =
                CreateContext();

            var service = new PetService(
                context,
                _loggerMock.Object,
                CreateMeterFactory());

            var tutor = new Tutor(
                fullName: "Maria Silva",
                email: "maria@email.com",
                passwordHash: "senha-hash",
                phoneNumber: "11999999999"
            );

            context.Tutors.Add(tutor);

            await context.SaveChangesAsync();

            var pet = new Pet(
                tutorId: tutor.UserId,
                name: "Luna",
                species: "Gato",
                breed: "SRD",
                sex: "Fêmea",
                age: 4,
                birthDate: new DateTime(2022, 5, 10)
            );

            // Act
            var result = await service.CreatePetAsync(pet);

            // Assert
            Assert.NotNull(result);

            Assert.True(result.PetId > 0);

            Assert.Equal(tutor.UserId,result.TutorId);

            Assert.Equal("Luna",result.Name);

            var petSalvo = await context.Pets.FirstOrDefaultAsync(p => p.PetId == result.PetId);

            Assert.NotNull(petSalvo);

            Assert.Equal("Luna",petSalvo.Name);

            Assert.Equal(tutor.UserId,petSalvo.TutorId);
        }
    }
}
