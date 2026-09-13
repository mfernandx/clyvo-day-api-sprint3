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
    public class UserControllerTests
    {
        private readonly ApiFactoryFixture _factory;
        private readonly HttpClient _client;

        public UserControllerTests(ApiFactoryFixture factory)
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

        private async Task<Veterinarian> CreateVeterinarianAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var veterinarian = new Veterinarian(
                "Carlos Souza",
                "carlos@email.com",
                "senha-hash",
                "11988888888",
                "12345",
                "SP",
                "Clínica Geral");

            context.Veterinarians.Add(veterinarian);
            await context.SaveChangesAsync();

            return veterinarian;
        }

        private void SetAuthenticatedUser(int userId)
        {
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        }

        [Fact]
        public async Task GetAllUsers_SemUsuarios_DeveRetornarOk()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/User");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAllUsers_ComUsuarios_DeveRetornarTodos()
        {
            await ResetDatabaseAsync();
            await CreateTutorAsync();
            await CreateVeterinarianAsync();

            var response = await _client.GetAsync("/api/User");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(2, json.GetArrayLength());
        }

        [Fact]
        public async Task GetAllUsers_FiltroTutor_DeveRetornarApenasTutores()
        {
            await ResetDatabaseAsync();
            await CreateTutorAsync();
            await CreateVeterinarianAsync();

            var response = await _client.GetAsync(
                $"/api/User?type={(int)EnumTypeUser.Tutor}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(1, json.GetArrayLength());
            Assert.Equal((int)EnumTypeUser.Tutor,json[0].GetProperty("typeUser").GetInt32());
        }

        [Fact]
        public async Task GetUserById_UsuarioExistente_DeveRetornarOk()
        {
            await ResetDatabaseAsync();
            var tutor = await CreateTutorAsync();

            SetAuthenticatedUser(tutor.UserId);

            var response = await _client.GetAsync(
                $"/api/User/{tutor.UserId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(tutor.UserId,
                json.GetProperty("userId").GetInt32());
            Assert.Equal("Maria Silva",
                json.GetProperty("fullName").GetString());
        }

        [Fact]
        public async Task GetUserById_UsuarioNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            SetAuthenticatedUser(1);

            var response = await _client.GetAsync("/api/User/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetUserById_IdInvalido_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();

            SetAuthenticatedUser(1);

            var response = await _client.GetAsync("/api/User/0");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEmail_DadosValidos_DeveRetornarOk()
        {
            await ResetDatabaseAsync();
            var tutor = await CreateTutorAsync();

            var body = new
            {
                email = "novo@email.com"
            };

            var response = await _client.PutAsJsonAsync(
                $"/api/User/{tutor.UserId}/email", body);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(tutor.UserId,
                json.GetProperty("userId").GetInt32());
            Assert.Equal("novo@email.com",
                json.GetProperty("email").GetString());
        }

        [Fact]
        public async Task UpdateEmail_EmailVazio_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();
            var tutor = await CreateTutorAsync();

            var body = new
            {
                email = ""
            };

            var response = await _client.PutAsJsonAsync(
                $"/api/User/{tutor.UserId}/email", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEmail_UsuarioNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            var body = new
            {
                email = "novo@email.com"
            };

            var response = await _client.PutAsJsonAsync(
                "/api/User/999/email", body);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdatePhoneNumber_DadosValidos_DeveRetornarOk()
        {
            await ResetDatabaseAsync();
            var tutor = await CreateTutorAsync();

            var body = new
            {
                phoneNumber = "11977777777"
            };

            var response = await _client.PutAsJsonAsync(
                $"/api/User/{tutor.UserId}/phone", body);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(tutor.UserId,
                json.GetProperty("userId").GetInt32());
            Assert.Equal("11977777777",
                json.GetProperty("phoneNumber").GetString());
        }

        [Fact]
        public async Task UpdatePhoneNumber_TelefoneVazio_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();
            var tutor = await CreateTutorAsync();

            var body = new
            {
                phoneNumber = ""
            };

            var response = await _client.PutAsJsonAsync(
                $"/api/User/{tutor.UserId}/phone", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeactivateUser_UsuarioExistente_DeveRetornarOk()
        {
            await ResetDatabaseAsync();
            var tutor = await CreateTutorAsync();

            var response = await _client.PutAsync(
                $"/api/User/{tutor.UserId}/deactivate",
                null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = await context.Users.FindAsync(tutor.UserId);

            Assert.NotNull(user);
            Assert.False(user.IsActive);
        }

        [Fact]
        public async Task DeactivateUser_UsuarioNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            var response = await _client.PutAsync(
                "/api/User/999/deactivate",
                null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_UsuarioExistente_DeveRetornarNoContent()
        {
            await ResetDatabaseAsync();
            var tutor = await CreateTutorAsync();

            var response = await _client.DeleteAsync(
                $"/api/User/{tutor.UserId}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = await context.Users.FindAsync(tutor.UserId);

            Assert.Null(user);
        }

        [Fact]
        public async Task DeleteUser_UsuarioNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            var response = await _client.DeleteAsync("/api/User/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
