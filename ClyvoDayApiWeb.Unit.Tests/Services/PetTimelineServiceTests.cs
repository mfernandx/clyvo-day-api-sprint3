
using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.MongoDB.Documents;
using ClyvoDayApiWeb.MongoDB.Repositories;
using ClyvoDayApiWeb.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ClyvoDayApiWeb.Unit.Tests.Services
{
    public class PetTimelineServiceTests
    {
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            return new AppDbContext(options);
        }

        private static async Task<Pet> CreatePetAsync(AppDbContext context)
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

            return pet;
        }

        private static PetTimelineEntry CreateEntry(int petId)
        {
            return new PetTimelineEntry
            {
                PetId = petId,
                Title = "Consulta veterinária",
                Category = "Saúde",
                Description = "Consulta de rotina",
                Tags = new List<string> { "consulta" },
                Details = new Dictionary<string, string>()
            };
        }

        [Fact]
        public async Task CreateAsync_TituloVazio_DeveLancarArgumentException()
        {
            // Arrange
            await using var context = CreateContext();
            var repositoryMock = new Mock<IPetTimelineRepository>();

            var service = new PetTimelineService(context, repositoryMock.Object);

            var entry = CreateEntry(1);
            entry.Title = "";

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(entry, 1));

            // Assert
            Assert.Equal("O título é obrigatório.", exception.Message);

            repositoryMock.Verify(r => r.CreateAsync(It.IsAny<PetTimelineEntry>()),Times.Never);
        }

        [Fact]
        public async Task CreateAsync_CategoriaVazia_DeveLancarArgumentException()
        {
            // Arrange
            await using var context = CreateContext();
            var repositoryMock = new Mock<IPetTimelineRepository>();

            var service = new PetTimelineService(context, repositoryMock.Object);

            var entry = CreateEntry(1);
            entry.Category = "";

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(entry, 1));

            // Assert
            Assert.Equal("A categoria é obrigatória.", exception.Message);
        }

        [Fact]
        public async Task CreateAsync_PetNaoPertenceAoTutor_DeveLancarKeyNotFoundException()
        {
            // Arrange
            await using var context = CreateContext();
            var repositoryMock = new Mock<IPetTimelineRepository>();

            var service = new PetTimelineService(context, repositoryMock.Object);

            var entry = CreateEntry(999);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAsync(entry, 1));

            // Assert
            Assert.Equal("Pet não encontrado ou não pertence ao tutor.", exception.Message);
        }

        [Fact]
        public async Task CreateAsync_DadosValidos_DeveCriarRegistro()
        {
            // Arrange
            await using var context = CreateContext();
            var pet = await CreatePetAsync(context);

            var repositoryMock = new Mock<IPetTimelineRepository>();

            var service = new PetTimelineService(context, repositoryMock.Object);

            var entry = CreateEntry(pet.PetId);

            // Act
            var result = await service.CreateAsync(entry, pet.TutorId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(pet.PetId, result.PetId);
            Assert.Equal(pet.TutorId, result.CreatedByUserId);
            Assert.Equal("Consulta veterinária", result.Title);
            Assert.True(result.RegisteredAt > DateTime.MinValue);

            repositoryMock.Verify(r => r.CreateAsync(entry),Times.Once);
        }

        [Fact]
        public async Task GetByPetIdAsync_PetNaoPertenceAoTutor_DeveLancarKeyNotFoundException()
        {
            // Arrange
            await using var context = CreateContext();
            var repositoryMock = new Mock<IPetTimelineRepository>();

            var service = new PetTimelineService(context, repositoryMock.Object);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByPetIdAsync(999, 1));

            // Assert
            Assert.Equal("Pet não encontrado ou não pertence ao tutor.", exception.Message);

            repositoryMock.Verify(r => r.GetByPetIdAsync(It.IsAny<int>()),Times.Never);
        }

        [Fact]
        public async Task GetByPetIdAsync_PetValido_DeveRetornarRegistros()
        {
            // Arrange
            await using var context = CreateContext();
            var pet = await CreatePetAsync(context);

            var entries = new List<PetTimelineEntry>
            {
                CreateEntry(pet.PetId)
            };

            var repositoryMock = new Mock<IPetTimelineRepository>();

            repositoryMock.Setup(r => r.GetByPetIdAsync(pet.PetId)).ReturnsAsync(entries);

            var service = new PetTimelineService(context, repositoryMock.Object);

            // Act
            var result = await service.GetByPetIdAsync(pet.PetId, pet.TutorId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Consulta veterinária", result[0].Title);

            repositoryMock.Verify(r => r.GetByPetIdAsync(pet.PetId),Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_RegistroInexistente_DeveRetornarNulo()
        {
            // Arrange
            await using var context = CreateContext();
            var repositoryMock = new Mock<IPetTimelineRepository>();

            repositoryMock.Setup(r => r.GetByIdAsync("id-inexistente")).ReturnsAsync((PetTimelineEntry?)null);

            var service = new PetTimelineService(context, repositoryMock.Object);

            // Act
            var result = await service.GetByIdAsync("id-inexistente", 1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_RegistroDeOutroTutor_DeveLancarKeyNotFoundException()
        {
            // Arrange
            await using var context = CreateContext();
            var pet = await CreatePetAsync(context);

            var repositoryMock = new Mock<IPetTimelineRepository>();

            var entry = CreateEntry(pet.PetId);

            repositoryMock.Setup(r => r.GetByIdAsync("registro-1")).ReturnsAsync(entry);

            var service = new PetTimelineService(context, repositoryMock.Object);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByIdAsync("registro-1", 999));

            // Assert
            Assert.Equal("Registro não encontrado.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_DadosValidos_DeveAtualizarRegistro()
        {
            // Arrange
            await using var context = CreateContext();
            var pet = await CreatePetAsync(context);

            var repositoryMock = new Mock<IPetTimelineRepository>();

            var existingEntry = CreateEntry(pet.PetId);
            existingEntry.Id = "registro-1";
            existingEntry.CreatedByUserId = pet.TutorId;
            existingEntry.RegisteredAt = new DateTime(2026, 1, 10);

            repositoryMock.Setup(r => r.GetByIdAsync("registro-1")).ReturnsAsync(existingEntry);

            var service = new PetTimelineService(context, repositoryMock.Object);

            var updatedEntry = CreateEntry(pet.PetId);
            updatedEntry.Title = "Consulta atualizada";

            // Act
            await service.UpdateAsync("registro-1", updatedEntry, pet.TutorId);

            // Assert
            Assert.Equal("registro-1", updatedEntry.Id);
            Assert.Equal(pet.TutorId, updatedEntry.CreatedByUserId);
            Assert.Equal(existingEntry.RegisteredAt, updatedEntry.RegisteredAt);
            Assert.Equal("Consulta atualizada", updatedEntry.Title);

            repositoryMock.Verify(r => r.UpdateAsync("registro-1", updatedEntry),Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_TentarAlterarPet_DeveLancarArgumentException()
        {
            // Arrange
            await using var context = CreateContext();
            var pet = await CreatePetAsync(context);

            var repositoryMock = new Mock<IPetTimelineRepository>();

            var existingEntry = CreateEntry(pet.PetId);

            repositoryMock.Setup(r => r.GetByIdAsync("registro-1")).ReturnsAsync(existingEntry);

            var service = new PetTimelineService(context, repositoryMock.Object);

            var updatedEntry = CreateEntry(999);

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateAsync("registro-1", updatedEntry, pet.TutorId));

            // Assert
            Assert.Equal("Não é permitido alterar o pet do registro.", exception.Message);

            repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<PetTimelineEntry>()),Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_RegistroValido_DeveExcluirRegistro()
        {
            // Arrange
            await using var context = CreateContext();
            var pet = await CreatePetAsync(context);

            var repositoryMock = new Mock<IPetTimelineRepository>();

            var entry = CreateEntry(pet.PetId);

            repositoryMock.Setup(r => r.GetByIdAsync("registro-1")).ReturnsAsync(entry);

            var service = new PetTimelineService(context, repositoryMock.Object);

            // Act
            await service.DeleteAsync("registro-1", pet.TutorId);

            // Assert
            repositoryMock.Verify(r => r.DeleteAsync("registro-1"),Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_RegistroDeOutroTutor_DeveLancarKeyNotFoundException()
        {
            // Arrange
            await using var context = CreateContext();
            var pet = await CreatePetAsync(context);

            var repositoryMock = new Mock<IPetTimelineRepository>();

            var entry = CreateEntry(pet.PetId);

            repositoryMock.Setup(r => r.GetByIdAsync("registro-1")).ReturnsAsync(entry);

            var service = new PetTimelineService(context, repositoryMock.Object);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.DeleteAsync("registro-1", 999));

            // Assert
            Assert.Equal("Registro não encontrado.", exception.Message);

            repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<string>()),Times.Never);
        }
    }
}

