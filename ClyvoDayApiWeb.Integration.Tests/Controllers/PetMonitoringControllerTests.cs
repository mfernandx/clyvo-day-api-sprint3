using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Integration.Tests.FactoryFixture;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClyvoDayApiWeb.IntegrationTests.Controllers
{
    [Collection("ApiCollection")]
    public class PetMonitoringControllerTests
    {
        private readonly ApiFactoryFixture _factory;
        private readonly HttpClient _client;

        public PetMonitoringControllerTests(ApiFactoryFixture factory)
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

            var tutor = new Tutor("Maria Silva", "maria@email.com", "senha-hash", "11999999999");
            context.Tutors.Add(tutor);
            await context.SaveChangesAsync();

            var pet = new Pet(tutor.UserId, "Luna", "Gato", "SRD", "Fêmea", 4, new DateTime(2022, 5, 10));
            context.Pets.Add(pet);
            await context.SaveChangesAsync();

            return (tutor, pet);
        }

        [Fact]
        public async Task GetAll_SemMonitoramentos_DeveRetornarOk()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/PetMonitoring");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_MonitoramentoNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/PetMonitoring/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_SemPetId_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();

            var body = new
            {
                mood = "Feliz",
                energyLevel = "Alta"
            };

            var response = await _client.PostAsJsonAsync("/api/PetMonitoring", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_DadosValidos_DeveRetornarCreated()
        {
            await ResetDatabaseAsync();
            var (tutor, pet) = await CreateTutorAndPetAsync();

            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", tutor.UserId.ToString());

            var body = new
            {
                petId = pet.PetId,
                mood = "Feliz",
                energyLevel = "Alta",
                hydrationLevel = "Boa",
                food = "Ração seca",
                sleepQuality = "Boa",
                recentActivities = "Passeio",
                sociability = "Sociável",
                tookMedication = false,
                weight = 5.5m,
                observations = "Sem alterações"
            };

            var response = await _client.PostAsJsonAsync("/api/PetMonitoring", body);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.True(json.GetProperty("petMonitoringId").GetInt32() > 0);
            Assert.Equal(pet.PetId, json.GetProperty("petId").GetInt32());
            Assert.Equal("Feliz", json.GetProperty("mood").GetString());
            Assert.Equal(5.5m, json.GetProperty("weight").GetDecimal());
        }

        [Fact]
        public async Task Create_UsuarioNaoEhTutorDoPet_DeveRetornarForbidden()
        {
            await ResetDatabaseAsync();
            var (_, pet) = await CreateTutorAndPetAsync();

            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", "999");

            var body = new
            {
                petId = pet.PetId,
                mood = "Feliz"
            };

            var response = await _client.PostAsJsonAsync("/api/PetMonitoring", body);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetByPetId_PetNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/PetMonitoring/pet/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
