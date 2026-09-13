using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Integration.Tests.FactoryFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClyvoDayApiWeb.IntegrationTests.Controllers
{
    [Collection("ApiCollection")]
    public class PetControllerTests
    {
        private readonly ApiFactoryFixture _factory;
        private readonly HttpClient _client;

        public PetControllerTests(ApiFactoryFixture factory)
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

        private async Task<Tutor> CreateTutorAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var tutor = new Tutor("Maria Silva", "maria@email.com", "senha-hash", "11999999999");
            context.Tutors.Add(tutor);
            await context.SaveChangesAsync();

            return tutor;
        }

        [Fact]
        public async Task GetAllPets_SemPets_DeveRetornarOk()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/Pet");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetPetById_IdInvalido_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/Pet/0");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetPetById_PetNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/Pet/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreatePet_DadosValidos_DeveRetornarCreated()
        {
            await ResetDatabaseAsync();
            var tutor = await CreateTutorAsync();

            var body = new
            {
                tutorId = tutor.UserId,
                name = "Luna",
                species = "Gato",
                breed = "SRD",
                sex = "Fêmea",
                age = 4,
                birthDate = new DateTime(2022, 5, 10)
            };

            var response = await _client.PostAsJsonAsync("/api/Pet", body);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.True(json.GetProperty("petId").GetInt32() > 0);
            Assert.Equal("Luna", json.GetProperty("name").GetString());
            Assert.Equal(tutor.UserId, json.GetProperty("tutorId").GetInt32());
        }

        [Fact]
        public async Task CreatePet_TutorNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            var body = new
            {
                tutorId = 999,
                name = "Luna",
                species = "Gato",
                breed = "SRD",
                sex = "Fêmea",
                age = 4,
                birthDate = new DateTime(2022, 5, 10)
            };

            var response = await _client.PostAsJsonAsync("/api/Pet", body);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
