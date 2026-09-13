using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Integration.Tests.FactoryFixture;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClyvoDayApiWeb.Integration.Tests.Controllers
{
    [Collection("ApiCollection")]
    public class TutorControllerTests
    {
        private readonly ApiFactoryFixture _factory;
        private readonly HttpClient _client;

        public TutorControllerTests(ApiFactoryFixture factory)
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

        private async Task<Tutor> CreateTutorAsync(string email = "maria@email.com")
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var tutor = new Tutor(
                "Maria Silva",
                email,
                "senha-hash",
                "11999999999");

            context.Tutors.Add(tutor);
            await context.SaveChangesAsync();

            return tutor;
        }

        [Fact]
        public async Task GetAllTutors_SemTutores_DeveRetornarOk()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/Tutor");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAllTutors_ComTutores_DeveRetornarTodos()
        {
            await ResetDatabaseAsync();
            await CreateTutorAsync();
            await CreateTutorAsync("ana@email.com");

            var response = await _client.GetAsync("/api/Tutor");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(2, json.GetArrayLength());
        }

        [Fact]
        public async Task GetTutorById_TutorExistente_DeveRetornarOk()
        {
            await ResetDatabaseAsync();
            var tutor = await CreateTutorAsync();

            var response = await _client.GetAsync($"/api/Tutor/{tutor.UserId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(tutor.UserId, json.GetProperty("userId").GetInt32());
            Assert.Equal("Maria Silva", json.GetProperty("fullName").GetString());
            Assert.Equal("maria@email.com", json.GetProperty("email").GetString());
        }

        [Fact]
        public async Task GetTutorById_TutorNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/Tutor/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetTutorById_IdInvalido_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/Tutor/0");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateTutor_DadosValidos_DeveRetornarCreated()
        {
            await ResetDatabaseAsync();

            var body = new
            {
                fullName = "Maria Silva",
                email = "maria@email.com",
                password = "123456",
                phoneNumber = "11999999999"
            };

            var response = await _client.PostAsJsonAsync("/api/Tutor", body);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.True(json.GetProperty("userId").GetInt32() > 0);
            Assert.Equal("Maria Silva", json.GetProperty("fullName").GetString());
            Assert.Equal("maria@email.com", json.GetProperty("email").GetString());
            Assert.Equal("11999999999", json.GetProperty("phoneNumber").GetString());
            Assert.Equal(0, json.GetProperty("scoreEngagement").GetInt32());
        }

        [Fact]
        public async Task CreateTutor_NomeVazio_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();

            var body = new
            {
                fullName = "",
                email = "maria@email.com",
                password = "123456",
                phoneNumber = "11999999999"
            };

            var response = await _client.PostAsJsonAsync("/api/Tutor", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateTutor_EmailVazio_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();

            var body = new
            {
                fullName = "Maria Silva",
                email = "",
                password = "123456",
                phoneNumber = "11999999999"
            };

            var response = await _client.PostAsJsonAsync("/api/Tutor", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateTutor_EmailDuplicado_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();
            await CreateTutorAsync();

            var body = new
            {
                fullName = "Outra Maria",
                email = "maria@email.com",
                password = "123456",
                phoneNumber = "11988888888"
            };

            var response = await _client.PostAsJsonAsync("/api/Tutor", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
