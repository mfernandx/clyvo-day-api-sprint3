using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Enums;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Integration.Tests.FactoryFixture;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClyvoDayApiWeb.Integration.Tests.Controllers
{
    [Collection("ApiCollection")]
    public class CareEventControllerTests
    {
        private readonly ApiFactoryFixture _factory;
        private readonly HttpClient _client;

        public CareEventControllerTests(ApiFactoryFixture factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task ResetDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
        }

        private async Task<(Tutor tutor, Pet pet)> CreateTutorAndPetAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var tutor = new Tutor(
                "Maria Silva",
                "maria@email.com",
                "senha-hash",
                "11999999999");

            context.Tutors.Add(tutor);
            await context.SaveChangesAsync();

            var pet = new Pet(
                tutor.UserId,
                "Luna",
                "Gato",
                "SRD",
                "Fêmea",
                4,
                new DateTime(2022, 5, 10));

            context.Pets.Add(pet);
            await context.SaveChangesAsync();

            return (tutor, pet);
        }

        private async Task<CareEvent> CreateCareEventAsync(int petId)
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var careEvent = new CareEvent(
                petId,
                "Vacinação",
                "Vacina B12",
                DateTime.UtcNow.AddDays(10),
                "Nenhuma");

            context.CareEvents.Add(careEvent);
            await context.SaveChangesAsync();

            return careEvent;
        }

        [Fact]
        public async Task GetAllCareEvents_SemEventos_DeveRetornarOk()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/CareEvent");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetCareEventById_EventoNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/CareEvent/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCareEventById_IdInvalido_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/CareEvent/0");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetCareEventsByPetId_PetNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/CareEvent/pet/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateCareEvent_DadosValidos_DeveRetornarCreated()
        {
            await ResetDatabaseAsync();
            var (_, pet) = await CreateTutorAndPetAsync();

            var body = new
            {
                petId = pet.PetId,
                typeEvent = "Vacinação",
                description = "Vacina B12",
                eventDate = DateTime.UtcNow.AddDays(10),
                observations = "Nenhuma"
            };

            var response = await _client.PostAsJsonAsync("/api/CareEvent", body);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.True(json.GetProperty("careEventId").GetInt32() > 0);
            Assert.Equal(pet.PetId, json.GetProperty("petId").GetInt32());
            Assert.Equal("Vacinação", json.GetProperty("typeEvent").GetString());
            Assert.Equal("Vacina B12", json.GetProperty("description").GetString());
        }

        [Fact]
        public async Task CreateCareEvent_PetNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            var body = new
            {
                petId = 999,
                typeEvent = "Vacinação",
                description = "Vacina B12",
                eventDate = DateTime.UtcNow.AddDays(10),
                observations = "Nenhuma"
            };

            var response = await _client.PostAsJsonAsync("/api/CareEvent", body);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CompleteCareEvent_EventoExistente_DeveRetornarOk()
        {
            await ResetDatabaseAsync();
            var (_, pet) = await CreateTutorAndPetAsync();
            var careEvent = await CreateCareEventAsync(pet.PetId);

            var response = await _client.PutAsync(
                $"/api/CareEvent/{careEvent.CareEventId}/complete",
                null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var updated = await context.CareEvents.FindAsync(careEvent.CareEventId);

            Assert.NotNull(updated);
            Assert.Equal(EnumCareEventStatus.Completed, updated.Status);
        }

        [Fact]
        public async Task CancelCareEvent_EventoExistente_DeveRetornarOk()
        {
            await ResetDatabaseAsync();
            var (_, pet) = await CreateTutorAndPetAsync();
            var careEvent = await CreateCareEventAsync(pet.PetId);

            var response = await _client.PutAsync(
                $"/api/CareEvent/{careEvent.CareEventId}/cancel",
                null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var updated = await context.CareEvents.FindAsync(careEvent.CareEventId);

            Assert.NotNull(updated);
            Assert.Equal(EnumCareEventStatus.Cancelled, updated.Status);
        }

        [Fact]
        public async Task CompleteCareEvent_EventoNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            var response = await _client.PutAsync(
                "/api/CareEvent/999/complete",
                null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CancelCareEvent_EventoNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            var response = await _client.PutAsync(
                "/api/CareEvent/999/cancel",
                null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCareEvent_EventoExistente_DeveRetornarOk()
        {
            await ResetDatabaseAsync();
            var (_, pet) = await CreateTutorAndPetAsync();
            var careEvent = await CreateCareEventAsync(pet.PetId);

            var response = await _client.DeleteAsync($"/api/CareEvent/{careEvent.CareEventId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var deleted = await context.CareEvents.FindAsync(careEvent.CareEventId);

            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteCareEvent_EventoNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            var response = await _client.DeleteAsync("/api/CareEvent/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
