
using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Integration.Tests.FactoryFixture;
using ClyvoDayApiWeb.MongoDB;
using ClyvoDayApiWeb.MongoDB.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClyvoDayApiWeb.Integration.Tests.Controllers
{
    [Collection("ApiCollection")]
    public class PetTimelineControllerTests
    {
        private readonly ApiFactoryFixture _factory;
        private readonly HttpClient _client;

        public PetTimelineControllerTests(ApiFactoryFixture factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private void SetTutorHeaders(int userId = 1)
        {
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Remove("X-Test-Role");
            _client.DefaultRequestHeaders.Remove("X-Test-Unauthenticated");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Tutor");
        }

        private async Task ResetDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
        }

        private async Task ResetMongoAsync()
        {
            using var scope = _factory.Services.CreateScope();

            var mongoContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();

            var collection = mongoContext.Database.GetCollection<PetTimelineEntry>("PetTimelineEntries");

            await collection.DeleteManyAsync(Builders<PetTimelineEntry>.Filter.Empty);
        }

        private async Task<Pet> CreatePetAsync()
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

            return pet;
        }

        private static object CreateEntryBody(int petId, string title = "Consulta veterinária")
        {
            return new
            {
                petId,
                title,
                category = "Saúde",
                description = "Consulta de rotina",
                tags = new[] { "consulta" },
                details = new Dictionary<string, string>
                {
                    { "veterinario", "Dr. João" }
                }
            };
        }

        [Fact]
        public async Task CreateTimeline_DadosValidos_DeveRetornarCreated()
        {
            // Arrange
            await ResetDatabaseAsync();
            await ResetMongoAsync();

            SetTutorHeaders();

            var pet = await CreatePetAsync();

            var body = CreateEntryBody(pet.PetId);

            // Act
            var response = await _client.PostAsJsonAsync("/api/PetTimeline", body);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("id").GetString()));
            Assert.Equal(pet.PetId, json.GetProperty("petId").GetInt32());
            Assert.Equal(pet.TutorId, json.GetProperty("createdByUserId").GetInt32());
            Assert.Equal("Consulta veterinária", json.GetProperty("title").GetString());
        }

        [Fact]
        public async Task CreateTimeline_TituloVazio_DeveRetornarBadRequest()
        {
            // Arrange
            await ResetDatabaseAsync();
            await ResetMongoAsync();

            SetTutorHeaders();

            var pet = await CreatePetAsync();

            var body = CreateEntryBody(pet.PetId, "");

            // Act
            var response = await _client.PostAsJsonAsync("/api/PetTimeline", body);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetTimelineByPetId_PetValido_DeveRetornarOk()
        {
            // Arrange
            await ResetDatabaseAsync();
            await ResetMongoAsync();

            SetTutorHeaders();

            var pet = await CreatePetAsync();

            await _client.PostAsJsonAsync("/api/PetTimeline", CreateEntryBody(pet.PetId));

            // Act
            var response = await _client.GetAsync($"/api/PetTimeline/pet/{pet.PetId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(1, json.GetArrayLength());
            Assert.Equal("Consulta veterinária", json[0].GetProperty("title").GetString());
        }

        [Fact]
        public async Task GetTimelineByPetId_PetDeOutroTutor_DeveRetornarNotFound()
        {
            // Arrange
            await ResetDatabaseAsync();
            await ResetMongoAsync();

            SetTutorHeaders(1);

            var pet = await CreatePetAsync();

            SetTutorHeaders(2);

            // Act
            var response = await _client.GetAsync($"/api/PetTimeline/pet/{pet.PetId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetTimelineById_IdInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            await ResetDatabaseAsync();
            await ResetMongoAsync();

            SetTutorHeaders();

            // Act
            var response = await _client.GetAsync("/api/PetTimeline/id-invalido");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateTimeline_DadosValidos_DeveRetornarNoContent()
        {
            // Arrange
            await ResetDatabaseAsync();
            await ResetMongoAsync();

            SetTutorHeaders();

            var pet = await CreatePetAsync();

            var createResponse = await _client.PostAsJsonAsync("/api/PetTimeline",CreateEntryBody(pet.PetId));

            var createdJson = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
            var id = createdJson.GetProperty("id").GetString();

            var updatedBody = CreateEntryBody(pet.PetId, "Consulta atualizada");

            // Act
            var response = await _client.PutAsJsonAsync($"/api/PetTimeline/{id}",updatedBody);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getResponse = await _client.GetAsync($"/api/PetTimeline/{id}");

            var updatedJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal("Consulta atualizada", updatedJson.GetProperty("title").GetString());
        }

        [Fact]
        public async Task DeleteTimeline_RegistroValido_DeveRetornarNoContent()
        {
            // Arrange
            await ResetDatabaseAsync();
            await ResetMongoAsync();

            SetTutorHeaders();

            var pet = await CreatePetAsync();

            var createResponse = await _client.PostAsJsonAsync("/api/PetTimeline", CreateEntryBody(pet.PetId));

            var createdJson = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
            var id = createdJson.GetProperty("id").GetString();

            // Act
            var response = await _client.DeleteAsync($"/api/PetTimeline/{id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getResponse = await _client.GetAsync($"/api/PetTimeline/{id}");

            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task AccessTimeline_Veterinario_DeveRetornarForbidden()
        {
            // Arrange
            await ResetDatabaseAsync();
            await ResetMongoAsync();

            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Remove("X-Test-Role");
            _client.DefaultRequestHeaders.Remove("X-Test-Unauthenticated");

            _client.DefaultRequestHeaders.Add("X-Test-UserId", "2");
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Veterinario");

            // Act
            var response = await _client.GetAsync("/api/PetTimeline/pet/1");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AccessTimeline_SemAutenticacao_DeveRetornarUnauthorized()
        {
            // Arrange
            await ResetDatabaseAsync();
            await ResetMongoAsync();

            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Remove("X-Test-Role");
            _client.DefaultRequestHeaders.Remove("X-Test-Unauthenticated");

            _client.DefaultRequestHeaders.Add("X-Test-Unauthenticated", "true");

            // Act
            var response = await _client.GetAsync("/api/PetTimeline/pet/1");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}

