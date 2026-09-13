using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Integration.Tests.FactoryFixture;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClyvoDayApiWeb.Integration.Tests.Controllers
{
    [Collection("ApiCollection")]
    public class AuthControllerTests
    {
        private readonly ApiFactoryFixture _factory;
        private readonly HttpClient _client;

        public AuthControllerTests(ApiFactoryFixture factory)
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

        private async Task<Tutor> CreateTutorAsync(string password = "123456")
        {
            using var scope = _factory.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

            var tutor = new Tutor(
                "Maria Silva",
                "maria@email.com",
                password,
                "11999999999");

            var passwordHash = passwordHasher.HashPassword(tutor, password);
            tutor.UpdatePasswordHash(passwordHash);

            context.Tutors.Add(tutor);
            await context.SaveChangesAsync();

            return tutor;
        }

        private void SetAuthenticatedUser(int userId)
        {
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        }

        [Fact]
        public async Task Login_CredenciaisValidas_DeveRetornarOk()
        {
            await ResetDatabaseAsync();
            var tutor = await CreateTutorAsync();

            var body = new
            {
                email = tutor.Email,
                password = "123456"
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/login", body);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(tutor.UserId, json.GetProperty("userId").GetInt32());
            Assert.Equal("Maria Silva", json.GetProperty("fullName").GetString());
            Assert.Equal("maria@email.com", json.GetProperty("email").GetString());
            Assert.Equal("Tutor", json.GetProperty("typeUser").GetString());
            Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("token").GetString()));
        }

        [Fact]
        public async Task Login_SenhaIncorreta_DeveRetornarUnauthorized()
        {
            await ResetDatabaseAsync();
            var tutor = await CreateTutorAsync();

            var body = new
            {
                email = tutor.Email,
                password = "senha-errada"
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/login", body);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_EmailNaoExiste_DeveRetornarUnauthorized()
        {
            await ResetDatabaseAsync();

            var body = new
            {
                email = "naoexiste@email.com",
                password = "123456"
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/login", body);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_EmailVazio_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();

            var body = new
            {
                email = "",
                password = "123456"
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/login", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_SenhaVazia_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();

            var body = new
            {
                email = "maria@email.com",
                password = ""
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/login", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAuthenticatedUser_TutorExistente_DeveRetornarOk()
        {
            await ResetDatabaseAsync();
            var tutor = await CreateTutorAsync();

            SetAuthenticatedUser(tutor.UserId);

            var response = await _client.GetAsync("/api/Auth/me");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(tutor.UserId, json.GetProperty("userId").GetInt32());
            Assert.Equal("Maria Silva", json.GetProperty("fullName").GetString());
            Assert.Equal("maria@email.com", json.GetProperty("email").GetString());
            Assert.Equal("Tutor", json.GetProperty("typeUser").GetString());
            Assert.Equal(0, json.GetProperty("scoreEngagement").GetInt32());
            Assert.Equal("Nenhum", json.GetProperty("achievement").GetString());
        }

        [Fact]
        public async Task GetAuthenticatedUser_UsuarioNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            SetAuthenticatedUser(999);

            var response = await _client.GetAsync("/api/Auth/me");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
