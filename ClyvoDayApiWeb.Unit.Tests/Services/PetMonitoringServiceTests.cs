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
    public class PetMonitoringServiceTests
    {
        private readonly Mock<ILogger<PetMonitoringService>> _loggerMock;

        public PetMonitoringServiceTests()
        {
            _loggerMock =
                new Mock<ILogger<PetMonitoringService>>();
        }

        private static AppDbContext CreateContext()
        {
            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(
                        Guid.NewGuid().ToString())
                    .Options;

            return new AppDbContext(options);
        }

        private static IMeterFactory CreateMeterFactory()
        {
            var services =
                new ServiceCollection();

            services.AddMetrics();

            var serviceProvider =
                services.BuildServiceProvider();

            return serviceProvider
                .GetRequiredService<IMeterFactory>();
        }

        private static async Task<(Tutor tutor, Pet pet)>
            CreateTutorAndPetAsync(AppDbContext context)
        {
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

            context.Pets.Add(pet);

            await context.SaveChangesAsync();

            return (tutor, pet);
        }

        [Fact]
        public async Task CreateAsync_MonitoramentoNulo_DeveLancarArgumentException()
        {
            // Arrange
            await using var context =
                CreateContext();

            var service =
                new PetMonitoringService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => service.CreateAsync(
                        null!,
                        1));

            // Assert
            Assert.Equal(
                "Os dados do monitoramento são obrigatórios.",
                exception.Message);
        }

        [Fact]
        public async Task CreateAsync_PetNaoExiste_DeveLancarInvalidOperationException()
        {
            // Arrange
            await using var context =
                CreateContext();

            var service =
                new PetMonitoringService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            var monitoring =
                new PetMonitoring(
                    petId: 999,
                    mood: "Feliz",
                    energyLevel: null,
                    hydrationLevel: null,
                    food: null,
                    sleepQuality: "",
                    recentActivities: null,
                    sociability: null,
                    tookMedication: null,
                    weight: null,
                    observations: null
                );

            // Act
            var exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => service.CreateAsync(
                        monitoring,
                        1));

            // Assert
            Assert.Equal(
                "Pet não encontrado.",
                exception.Message);
        }

        [Fact]
        public async Task CreateAsync_UsuarioNaoEhTutorDoPet_DeveLancarUnauthorizedAccessException()
        {
            // Arrange
            await using var context =
                CreateContext();

            var (_, pet) =
                await CreateTutorAndPetAsync(context);

            var service =
                new PetMonitoringService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            var monitoring =
                new PetMonitoring(
                    petId: pet.PetId,
                    mood: "Feliz",
                    energyLevel: null,
                    hydrationLevel: null,
                    food: null,
                    sleepQuality: "",
                    recentActivities: null,
                    sociability: null,
                    tookMedication: null,
                    weight: null,
                    observations: null
                );

            var outroUsuarioId = 999;

            // Act
            var exception =
                await Assert.ThrowsAsync<UnauthorizedAccessException>(
                    () => service.CreateAsync(
                        monitoring,
                        outroUsuarioId));

            // Assert
            Assert.Equal(
                "Você não pode criar monitoramentos para este pet.",
                exception.Message);
        }

        [Fact]
        public async Task CreateAsync_NenhumCampoPreenchido_DeveLancarArgumentException()
        {
            // Arrange
            await using var context =
                CreateContext();

            var (tutor, pet) =
                await CreateTutorAndPetAsync(context);

            var service =
                new PetMonitoringService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            var monitoring =
                new PetMonitoring(
                    petId: pet.PetId,
                    mood: null,
                    energyLevel: null,
                    hydrationLevel: null,
                    food: null,
                    sleepQuality: "",
                    recentActivities: null,
                    sociability: null,
                    tookMedication: null,
                    weight: null,
                    observations: null
                );

            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => service.CreateAsync(
                        monitoring,
                        tutor.UserId));

            // Assert
            Assert.Equal(
                "Preencha pelo menos um campo do monitoramento.",
                exception.Message);
        }

        [Fact]
        public async Task CreateAsync_PesoInvalido_DeveLancarArgumentException()
        {
            // Arrange
            await using var context =
                CreateContext();

            var (tutor, pet) =
                await CreateTutorAndPetAsync(context);

            var service =
                new PetMonitoringService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            var monitoring =
                new PetMonitoring(
                    petId: pet.PetId,
                    mood: null,
                    energyLevel: null,
                    hydrationLevel: null,
                    food: null,
                    sleepQuality: "",
                    recentActivities: null,
                    sociability: null,
                    tookMedication: null,
                    weight: 0,
                    observations: null
                );

            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => service.CreateAsync(
                        monitoring,
                        tutor.UserId));

            // Assert
            Assert.Equal(
                "O peso deve ser maior que zero.",
                exception.Message);
        }

        [Fact]
        public async Task CreateAsync_DadosValidos_DeveCriarMonitoramento()
        {
            // Arrange
            await using var context =
                CreateContext();

            var (tutor, pet) =
                await CreateTutorAndPetAsync(context);

            var service =
                new PetMonitoringService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            var monitoring =
                new PetMonitoring(
                    petId: pet.PetId,
                    mood: "Feliz",
                    energyLevel: "Alta",
                    hydrationLevel: "Boa",
                    food: "Ração seca",
                    sleepQuality: "Boa",
                    recentActivities: "Passeio",
                    sociability: "Sociável",
                    tookMedication: false,
                    weight: 5.5m,
                    observations: "Sem alterações"
                );

            var scoreAntes =
                tutor.ScoreEngagement;

            // Act
            var result =
                await service.CreateAsync(
                    monitoring,
                    tutor.UserId);

            // Assert
            Assert.NotNull(result);

            Assert.True(
                result.PetMonitoringId > 0);

            Assert.Equal(
                pet.PetId,
                result.PetId);

            Assert.Equal(
                "Feliz",
                result.Mood);

            Assert.Equal(
                5.5m,
                result.Weight);

            var monitoringSalvo =
                await context.PetMonitorings
                    .FirstOrDefaultAsync(
                        p => p.PetMonitoringId ==
                             result.PetMonitoringId);

            Assert.NotNull(
                monitoringSalvo);

            Assert.Equal(
                pet.PetId,
                monitoringSalvo.PetId);

            Assert.Equal(
                "Feliz",
                monitoringSalvo.Mood);

            Assert.True(
                tutor.ScoreEngagement >
                scoreAntes);
        }
    }
}
