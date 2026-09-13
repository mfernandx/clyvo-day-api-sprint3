using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Constants;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.Metrics;

namespace ClyvoDayApiWeb.Unit.Tests.Services
{
    public class CommunityPostServiceTests
    {
        private readonly Mock<ILogger<CommunityPostService>> _loggerMock;

        public CommunityPostServiceTests()
        {
            _loggerMock = new Mock<ILogger<CommunityPostService>>();
        }

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            return new AppDbContext(options);
        }

        private static IMeterFactory CreateMeterFactory()
        {
            var services = new ServiceCollection();

            services.AddMetrics();

            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider.GetRequiredService<IMeterFactory>();
        }

        private static async Task<Tutor> CreateTutorAsync(AppDbContext context)
        {
            var tutor = new Tutor(
                fullName: "Maria Silva",
                email: "maria@email.com",
                passwordHash: "senha-hash",
                phoneNumber: "11999999999"
            );

            context.Tutors.Add(tutor);

            await context.SaveChangesAsync();

            return tutor;
        }

        [Fact]
        public async Task CreateAsync_PublicacaoNula_DeveLancarArgumentException()
        {
            // Arrange
            await using var context = CreateContext();

            var service =
                new CommunityPostService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(null!));

            // Assert
            Assert.Equal("Os dados da publicação são obrigatórios.",exception.Message);
        }

        [Fact]
        public async Task CreateAsync_UsuarioNaoExiste_DeveLancarInvalidOperationException()
        {
            // Arrange
            await using var context = CreateContext();

            var service =
                new CommunityPostService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            var post =
                new CommunityPost(
                    userId: 999,
                    category: "Dicas",
                    content: "Hoje fizemos um passeio muito legal.",
                    imageUrl: null,
                    location: "São Paulo"
                );

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(post));

            // Assert
            Assert.Equal("Usuário não encontrado ou inativo.",exception.Message);
        }

        [Fact]
        public async Task CreateAsync_CategoriaVazia_DeveLancarArgumentException()
        {
            // Arrange
            await using var context = CreateContext();

            var tutor = await CreateTutorAsync(context);

            var service =
                new CommunityPostService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            var post =
                new CommunityPost(
                    userId: tutor.UserId,
                    category: "",
                    content: "Hoje fizemos um passeio muito legal.",
                    imageUrl: null,
                    location: "São Paulo"
                );

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(post));

            // Assert
            Assert.Equal("A categoria da publicação é obrigatória.",exception.Message);
        }

        [Fact]
        public async Task CreateAsync_ConteudoVazio_DeveLancarArgumentException()
        {
            // Arrange
            await using var context = CreateContext();

            var tutor = await CreateTutorAsync(context);

            var service =
                new CommunityPostService(
                    context,
                    _loggerMock.Object,
                    CreateMeterFactory());

            var post =
                new CommunityPost(
                    userId: tutor.UserId,
                    category: "Dicas",
                    content: "",
                    imageUrl: null,
                    location: "São Paulo"
                );

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(post));

            // Assert
            Assert.Equal("O conteúdo da publicação é obrigatório.", exception.Message);
        }

        [Fact]
        public async Task CreateAsync_DadosValidos_DeveCriarPublicacao()
        {
            // Arrange
            await using var context = CreateContext();

            var tutor = await CreateTutorAsync(context);

            var service = new CommunityPostService(context,_loggerMock.Object,CreateMeterFactory());

            var post =
                new CommunityPost(
                    userId: tutor.UserId,
                    category: "Dicas",
                    content: "Hoje fizemos um passeio muito legal.",
                    imageUrl: null,
                    location: "São Paulo"
                );

            var scoreAntes = tutor.ScoreEngagement;

            // Act
            var result = await service.CreateAsync(post);

            // Assert
            Assert.NotNull(result);

            Assert.True(result.CommunityPostId > 0);

            Assert.Equal(tutor.UserId,result.UserId);

            Assert.Equal("Dicas",result.Category);

            Assert.Equal("Hoje fizemos um passeio muito legal.",result.Content);

            var postSalvo = await context.CommunityPosts.FirstOrDefaultAsync(p => p.CommunityPostId == result.CommunityPostId);

            Assert.NotNull(postSalvo);

            Assert.Equal(tutor.UserId,postSalvo.UserId);

            Assert.Equal("Dicas",postSalvo.Category);

            Assert.Equal(scoreAntes + EngagementPoints.CommunityPost,tutor.ScoreEngagement);
        }
    }
}
