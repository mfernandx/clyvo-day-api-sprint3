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
    public class VeterinarianControllerTests
    {
        private readonly ApiFactoryFixture _factory;
        private readonly HttpClient _client;

        public VeterinarianControllerTests(ApiFactoryFixture factory)
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

        private async Task<Veterinarian> CreateVeterinarianAsync(string email = "carlos@email.com")
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var veterinarian = new Veterinarian(
                "Carlos Souza",
                email,
                "senha-hash",
                "11988888888",
                "12345",
                "SP",
                "Clínica Geral");

            context.Veterinarians.Add(veterinarian);
            await context.SaveChangesAsync();

            return veterinarian;
        }

        [Fact]
        public async Task GetAllVeterinarians_SemVeterinarios_DeveRetornarOk()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/Veterinarian");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAllVeterinarians_ComVeterinarios_DeveRetornarTodos()
        {
            await ResetDatabaseAsync();
            await CreateVeterinarianAsync();
            await CreateVeterinarianAsync("ana@email.com");

            var response = await _client.GetAsync("/api/Veterinarian");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(2, json.GetArrayLength());
        }

        [Fact]
        public async Task GetVeterinarianById_VeterinarioExistente_DeveRetornarOk()
        {
            await ResetDatabaseAsync();
            var veterinarian = await CreateVeterinarianAsync();

            var response = await _client.GetAsync($"/api/Veterinarian/{veterinarian.UserId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(veterinarian.UserId, json.GetProperty("userId").GetInt32());
            Assert.Equal("Carlos Souza", json.GetProperty("fullName").GetString());
            Assert.Equal("carlos@email.com", json.GetProperty("email").GetString());
            Assert.Equal("12345", json.GetProperty("crmv").GetString());
            Assert.Equal("SP", json.GetProperty("state").GetString());
            Assert.Equal("Clínica Geral", json.GetProperty("specialty").GetString());
        }

        [Fact]
        public async Task GetVeterinarianById_VeterinarioNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/Veterinarian/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetVeterinarianById_IdInvalido_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/Veterinarian/0");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateVeterinarian_DadosValidos_DeveRetornarCreated()
        {
            await ResetDatabaseAsync();

            var body = new
            {
                fullName = "Carlos Souza",
                email = "carlos@email.com",
                password = "123456",
                phoneNumber = "11988888888",
                crmv = "12345",
                state = "SP",
                specialty = "Clínica Geral"
            };

            var response = await _client.PostAsJsonAsync("/api/Veterinarian", body);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.True(json.GetProperty("userId").GetInt32() > 0);
            Assert.Equal("Carlos Souza", json.GetProperty("fullName").GetString());
            Assert.Equal("carlos@email.com", json.GetProperty("email").GetString());
            Assert.Equal("11988888888", json.GetProperty("phoneNumber").GetString());
            Assert.Equal("12345", json.GetProperty("crmv").GetString());
            Assert.Equal("SP", json.GetProperty("state").GetString());
            Assert.Equal("Clínica Geral", json.GetProperty("specialty").GetString());
        }

        [Fact]
        public async Task CreateVeterinarian_NomeVazio_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();

            var body = new
            {
                fullName = "",
                email = "carlos@email.com",
                password = "123456",
                phoneNumber = "11988888888",
                crmv = "12345",
                state = "SP",
                specialty = "Clínica Geral"
            };

            var response = await _client.PostAsJsonAsync("/api/Veterinarian", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateVeterinarian_EmailVazio_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();

            var body = new
            {
                fullName = "Carlos Souza",
                email = "",
                password = "123456",
                phoneNumber = "11988888888",
                crmv = "12345",
                state = "SP",
                specialty = "Clínica Geral"
            };

            var response = await _client.PostAsJsonAsync("/api/Veterinarian", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateVeterinarian_EmailDuplicado_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();
            await CreateVeterinarianAsync();

            var body = new
            {
                fullName = "Outro Veterinário",
                email = "carlos@email.com",
                password = "123456",
                phoneNumber = "11977777777",
                crmv = "99999",
                state = "SP",
                specialty = "Dermatologia"
            };

            var response = await _client.PostAsJsonAsync("/api/Veterinarian", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
