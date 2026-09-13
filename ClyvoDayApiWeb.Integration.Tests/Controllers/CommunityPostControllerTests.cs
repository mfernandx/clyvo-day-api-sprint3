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
    public class CommunityPostControllerTests
    {
        private readonly ApiFactoryFixture _factory;
        private readonly HttpClient _client;

        public CommunityPostControllerTests(ApiFactoryFixture factory)
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

            var tutor = new Tutor(
                "Maria Silva",
                "maria@email.com",
                "senha-hash",
                "11999999999");

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
        public async Task GetAll_SemPublicacoes_DeveRetornarOk()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/CommunityPost");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_PublicacaoNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();

            var response = await _client.GetAsync("/api/CommunityPost/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetByUserId_UsuarioValido_DeveRetornarOk()
        {
            await ResetDatabaseAsync();
            var tutor = await CreateTutorAsync();

            var response = await _client.GetAsync($"/api/CommunityPost/user/{tutor.UserId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Create_CategoriaVazia_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();
            var tutor = await CreateTutorAsync();
            SetAuthenticatedUser(tutor.UserId);

            var body = new
            {
                category = "",
                content = "Publicação de teste.",
                imageUrl = (string?)null,
                location = "São Paulo"
            };

            var response = await _client.PostAsJsonAsync("/api/CommunityPost", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_ConteudoVazio_DeveRetornarBadRequest()
        {
            await ResetDatabaseAsync();
            var tutor = await CreateTutorAsync();
            SetAuthenticatedUser(tutor.UserId);

            var body = new
            {
                category = "Dicas",
                content = "",
                imageUrl = (string?)null,
                location = "São Paulo"
            };

            var response = await _client.PostAsJsonAsync("/api/CommunityPost", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_DadosValidos_DeveRetornarCreated()
        {
            await ResetDatabaseAsync();
            var tutor = await CreateTutorAsync();
            SetAuthenticatedUser(tutor.UserId);

            var body = new
            {
                category = "Dicas",
                content = "Hoje fizemos um passeio muito legal.",
                imageUrl = (string?)null,
                location = "São Paulo"
            };

            var response = await _client.PostAsJsonAsync("/api/CommunityPost", body);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.True(json.GetProperty("communityPostId").GetInt32() > 0);
            Assert.Equal(tutor.UserId, json.GetProperty("userId").GetInt32());
            Assert.Equal("Dicas", json.GetProperty("category").GetString());
            Assert.Equal("Hoje fizemos um passeio muito legal.", json.GetProperty("content").GetString());
            Assert.Equal("São Paulo", json.GetProperty("location").GetString());
        }

        [Fact]
        public async Task Delete_PublicacaoNaoExiste_DeveRetornarNotFound()
        {
            await ResetDatabaseAsync();
            var tutor = await CreateTutorAsync();
            SetAuthenticatedUser(tutor.UserId);

            var response = await _client.DeleteAsync("/api/CommunityPost/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
