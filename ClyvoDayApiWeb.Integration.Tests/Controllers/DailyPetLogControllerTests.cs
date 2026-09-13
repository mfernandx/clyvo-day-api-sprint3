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
    public class DailyPetLogControllerTests
    {
        private readonly ApiFactoryFixture _factory;
        private readonly HttpClient _client;

        public DailyPetLogControllerTests(ApiFactoryFixture factory)
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

        [Fact]
        public async Task GetAll_SemRegistros_DeveRetornarOk()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/DailyPetLog");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_RegistroNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/DailyPetLog/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetByPetId_PetNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/DailyPetLog/pet/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_ConteudoVazio_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();
            var (tutor, pet) = await CreateTutorAndPetAsync();

            SetAuthenticatedUser(tutor.UserId);

            var body = new
            {
                petId = pet.PetId,
                dailyPetLogType = "Comportamento",
                content = "",
                privacy = (int)EnumPrivacy.Privado,
                imageUrl = (string?)null
            };

            var response = await _client.PostAsJsonAsync("/api/DailyPetLog", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_DadosValidos_DeveRetornarCreated()
        {
            await ResetDatabaseAsync();
            var (tutor, pet) = await CreateTutorAndPetAsync();

            SetAuthenticatedUser(tutor.UserId);

            var body = new
            {
                petId = pet.PetId,
                dailyPetLogType = "Comportamento",
                content = "Luna ficou tranquila durante o dia.",
                privacy = (int)EnumPrivacy.Privado,
                imageUrl = (string?)null
            };

            var response = await _client.PostAsJsonAsync("/api/DailyPetLog", body);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.True(json.GetProperty("dailyPetLogId").GetInt32() > 0);
            Assert.Equal(pet.PetId, json.GetProperty("petId").GetInt32());
            Assert.Equal(tutor.UserId, json.GetProperty("createdByUserId").GetInt32());
            Assert.Equal("Comportamento", json.GetProperty("dailyPetLogType").GetString());
            Assert.Equal("Luna ficou tranquila durante o dia.", json.GetProperty("content").GetString());
        }

        [Fact]
        public async Task Create_UsuarioNaoEhTutorDoPet_DeveRetornarForbidden()
        {
            await ResetDatabaseAsync();
            var (_, pet) = await CreateTutorAndPetAsync();

            SetAuthenticatedUser(999);

            var body = new
            {
                petId = pet.PetId,
                dailyPetLogType = "Comportamento",
                content = "Registro de teste.",
                privacy = (int)EnumPrivacy.Privado,
                imageUrl = (string?)null
            };

            var response = await _client.PostAsJsonAsync("/api/DailyPetLog", body);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private void SetAuthenticatedUser(int userId)
        {
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        }
    }
}
