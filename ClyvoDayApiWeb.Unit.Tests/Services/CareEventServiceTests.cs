using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Enums;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.Metrics;

namespace ClyvoDayApiWeb.Unit.Tests.Services
{
    public class CareEventServiceTests
    {
        private readonly Mock<ILogger<CareEventService>> _loggerMock;

        public CareEventServiceTests()
        {
            _loggerMock = new Mock<ILogger<CareEventService>>();
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
        public async Task CreateCareEventAsync_EventoNulo_DeveLancarArgumentException()
        {
            // Arrange
            await using var context =
                CreateContext();

            var service =
                new CareEventService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => service.CreateCareEventAsync(null!));

            // Assert
            Assert.Equal(
                "Os dados do evento são obrigatórios.",
                exception.Message);
        }

        [Fact]
        public async Task CreateCareEventAsync_PetIdInvalido_DeveLancarArgumentException()
        {
            // Arrange
            await using var context =
                CreateContext();

            var service =
                new CareEventService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            var careEvent =
                new CareEvent(
                    petId: 0,
                    typeEvent: "Vacinação",
                    description: "Vacina B12",
                    eventDate: DateTime.UtcNow.AddDays(10),
                    observations: "Nenhuma"
                );

            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => service.CreateCareEventAsync(
                        careEvent));

            // Assert
            Assert.Equal(
                "O pet do evento é obrigatório.",
                exception.Message);
        }

        [Fact]
        public async Task CreateCareEventAsync_PetNaoExiste_DeveLancarKeyNotFoundException()
        {
            // Arrange
            await using var context =
                CreateContext();

            var service =
                new CareEventService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            var careEvent =
                new CareEvent(
                    petId: 999,
                    typeEvent: "Vacinação",
                    description: "Vacina B12",
                    eventDate: DateTime.UtcNow.AddDays(10),
                    observations: "Nenhuma"
                );

            // Act
            var exception =
                await Assert.ThrowsAsync<KeyNotFoundException>(
                    () => service.CreateCareEventAsync(
                        careEvent));

            // Assert
            Assert.Equal(
                "Pet não encontrado.",
                exception.Message);
        }

        [Fact]
        public async Task CreateCareEventAsync_DescricaoVazia_DeveLancarArgumentException()
        {
            // Arrange
            await using var context =
                CreateContext();

            var (_, pet) =
                await CreateTutorAndPetAsync(context);

            var service =
                new CareEventService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            var careEvent =
                new CareEvent(
                    petId: pet.PetId,
                    typeEvent: "Vacinação",
                    description: "",
                    eventDate: DateTime.UtcNow.AddDays(10),
                    observations: "Nenhuma"
                );

            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => service.CreateCareEventAsync(
                        careEvent));

            // Assert
            Assert.Equal(
                "A descrição do evento é obrigatória.",
                exception.Message);
        }

        [Fact]
        public async Task CreateCareEventAsync_DadosValidos_DeveCriarEvento()
        {
            // Arrange
            await using var context =
                CreateContext();

            var (_, pet) =
                await CreateTutorAndPetAsync(context);

            var service =
                new CareEventService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            var eventDate =
                DateTime.UtcNow.AddDays(10);

            var careEvent =
                new CareEvent(
                    petId: pet.PetId,
                    typeEvent: "Vacinação",
                    description: "Vacina B12",
                    eventDate: eventDate,
                    observations: "Nenhuma"
                );

            // Act
            var result =
                await service.CreateCareEventAsync(
                    careEvent);

            // Assert
            Assert.NotNull(result);

            Assert.True(
                result.CareEventId > 0);

            Assert.Equal(
                pet.PetId,
                result.PetId);

            Assert.Equal(
                "Vacinação",
                result.TypeEvent);

            Assert.Equal(
                "Vacina B12",
                result.Description);

            Assert.Equal(
                EnumCareEventStatus.Scheduled,
                result.Status);

            var eventoSalvo =
                await context.CareEvents
                    .FirstOrDefaultAsync(
                        c => c.CareEventId ==
                             result.CareEventId);

            Assert.NotNull(eventoSalvo);

            Assert.Equal(
                pet.PetId,
                eventoSalvo.PetId);

            Assert.Equal(
                "Vacinação",
                eventoSalvo.TypeEvent);

            Assert.Equal(
                EnumCareEventStatus.Scheduled,
                eventoSalvo.Status);
        }
    }
}
