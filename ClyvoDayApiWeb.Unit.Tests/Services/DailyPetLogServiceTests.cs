using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Constants;
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
    public class DailyPetLogServiceTests
    {
        private readonly Mock<ILogger<DailyPetLogService>> _loggerMock;

        public DailyPetLogServiceTests()
        {
            _loggerMock = new Mock<ILogger<DailyPetLogService>>();
        }

        private static AppDbContext CreateContext()
        {
            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;

            return new AppDbContext(options);
        }

        private static IMeterFactory CreateMeterFactory()
        {
            var services = new ServiceCollection();

            services.AddMetrics();

            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider.GetRequiredService<IMeterFactory>();
        }

        private static async Task<(Tutor tutor, Pet pet)>CreateTutorAndPetAsync(AppDbContext context)
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
        public async Task CreateAsync_RegistroNulo_DeveLancarArgumentException()
        {
            // Arrange
            await using var context = CreateContext();

            var service =
                new DailyPetLogService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(null!,1));

            // Assert
            Assert.Equal("Os dados do registro diário são obrigatórios.",exception.Message);
        }

        [Fact]
        public async Task CreateAsync_PetNaoExiste_DeveLancarInvalidOperationException()
        {
            // Arrange
            await using var context = CreateContext();

            var service =
                new DailyPetLogService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            var dailyPetLog =
                new DailyPetLog(
                    petId: 999,
                    createdByUserId: 1,
                    dailyPetLogType: "Comportamento",
                    content: "Pet ficou tranquilo durante o dia.",
                    imageUrl: "",
                    privacy: EnumPrivacy.Privado
                );

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dailyPetLog,1));

            // Assert
            Assert.Equal("Pet não encontrado.",exception.Message);
        }

        [Fact]
        public async Task CreateAsync_UsuarioNaoEhTutorDoPet_DeveLancarUnauthorizedAccessException()
        {
            // Arrange
            await using var context = CreateContext();

            var (_, pet) = await CreateTutorAndPetAsync(context);

            var service =
                new DailyPetLogService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            var dailyPetLog =
                new DailyPetLog(
                    petId: pet.PetId,
                    createdByUserId: 999,
                    dailyPetLogType: "Comportamento",
                    content: "Pet ficou tranquilo durante o dia.",
                    imageUrl: "",
                    privacy: EnumPrivacy.Privado
                );

            var outroUsuarioId = 999;

            // Act
            var exception =
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateAsync(dailyPetLog,outroUsuarioId));

            // Assert
            Assert.Equal("Você não pode criar registros para este pet.",exception.Message);
        }

        [Fact]
        public async Task CreateAsync_ConteudoVazio_DeveLancarArgumentException()
        {
            // Arrange
            await using var context = CreateContext();

            var (tutor, pet) = await CreateTutorAndPetAsync(context);

            var service =
                new DailyPetLogService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            var dailyPetLog =
                new DailyPetLog(
                    petId: pet.PetId,
                    createdByUserId: tutor.UserId,
                    dailyPetLogType: "Comportamento",
                    content: "",
                    imageUrl: "",
                    privacy: EnumPrivacy.Privado
                );

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dailyPetLog,tutor.UserId));

            // Assert
            Assert.Equal("O conteúdo do registro é obrigatório.",exception.Message);
        }

        [Fact]
        public async Task CreateAsync_DadosValidos_DeveCriarRegistroDiario()
        {
            // Arrange
            await using var context = CreateContext();

            var (tutor, pet) = await CreateTutorAndPetAsync(context);

            var service =
                new DailyPetLogService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            var dailyPetLog =
                new DailyPetLog(
                    petId: pet.PetId,
                    createdByUserId: tutor.UserId,
                    dailyPetLogType: "Comportamento",
                    content: "Luna ficou tranquila durante o dia.",
                    imageUrl: "",
                    privacy: EnumPrivacy.Privado
                );

            var scoreAntes = tutor.ScoreEngagement;

            // Act
            var result = await service.CreateAsync(dailyPetLog,tutor.UserId);

            // Assert
            Assert.NotNull(result);

            Assert.True(result.DailyPetLogId > 0);

            Assert.Equal(pet.PetId,result.PetId);

            Assert.Equal(tutor.UserId,result.CreatedByUserId);

            Assert.Equal("Comportamento",result.DailyPetLogType);

            Assert.Equal("Luna ficou tranquila durante o dia.",result.Content);

            var registroSalvo = await context.DailyPetLogs.FirstOrDefaultAsync(d => d.DailyPetLogId == result.DailyPetLogId);

            Assert.NotNull(registroSalvo);

            Assert.Equal(pet.PetId,registroSalvo.PetId);

            Assert.Equal(tutor.UserId,registroSalvo.CreatedByUserId);

            Assert.Equal("Luna ficou tranquila durante o dia.",registroSalvo.Content);

            Assert.Equal(scoreAntes + EngagementPoints.DailyPetLog,tutor.ScoreEngagement);
        }
    }
}
