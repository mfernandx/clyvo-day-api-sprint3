using ClyvoDayApiWeb.Data;
using ClyvoDayApiWeb.Domain.Constants;
using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Infrastructure.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ClyvoDayApiWeb.Services
{
    public class CommunityPostService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CommunityPostService> _logger;
        private static readonly ActivitySource ActivitySource =new(TelemetryConstants.ServiceName);
        private readonly Counter<int> _communityPostsCreatedCounter;


        public CommunityPostService(AppDbContext context, ILogger<CommunityPostService> logger, IMeterFactory meterFactory)
        {
            _context = context;
            _logger = logger;
            var meter = meterFactory.Create(TelemetryConstants.MeterName);
            _communityPostsCreatedCounter = meter.CreateCounter<int>("community_posts_created_total",description: "Total de publicações da comunidade criadas");
        }

        public async Task<List<CommunityPost>> GetAllAsync()
        {
            _logger.LogInformation("Buscando todos os posts da comunidade.");

            var communityPosts = await _context.CommunityPosts.AsNoTracking().Include(cp => cp.User).OrderByDescending(cp => cp.RegisteredAt).ToListAsync();

            _logger.LogInformation("Busca de posts da comunidade concluída. Total encontrado: {TotalCommunityPosts}.", communityPosts.Count);

            return communityPosts;

        }

        public async Task<CommunityPost?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Buscando publicação da comunidade com ID {CommunityPostId}.", id);

            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de buscar publicação com ID inválido: {CommunityPostId}.",id);
                throw new ArgumentException("O ID da publicação deve ser maior que zero.");
            }
                

            var communityPost = await _context.CommunityPosts.AsNoTracking().Include(cp => cp.User).FirstOrDefaultAsync(cp => cp.CommunityPostId == id);

            if (communityPost == null)
            {
                _logger.LogWarning("Publicação com ID {CommunityPostId} não encontrada.",id);
            }
            else
            {
                _logger.LogInformation("Publicação com ID {CommunityPostId} encontrada com sucesso.",id);
            }

            return communityPost;
        }

        public async Task<List<CommunityPost>> GetByUserIdAsync(int userId)
        {
            _logger.LogInformation("Buscando publicações do usuário {UserId}.",userId);

            if (userId <= 0)
            {
                _logger.LogWarning("Tentativa de buscar publicações com UserId inválido: {UserId}.",userId);

                throw new ArgumentException("O ID do usuário deve ser maior que zero.");
            }
                

            var communityPosts = await _context.CommunityPosts.AsNoTracking().Include(cp => cp.User).Where(cp => cp.UserId == userId).OrderByDescending(cp => cp.RegisteredAt).ToListAsync();

            _logger.LogInformation("Busca de publicações concluída para o usuário {UserId}. Total encontrado: {TotalCommunityPosts}.",userId, communityPosts.Count);

            return communityPosts;

        }

        public async Task<CommunityPost> CreateAsync(CommunityPost communityPost)
        {
            using var activity = ActivitySource.StartActivity("CreateCommunityPost");

            activity?.SetTag("user.id",communityPost?.UserId);

            _logger.LogInformation("Iniciando criação de publicação para o usuário {UserId}.", communityPost?.UserId);

            if (communityPost == null)
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Dados da publicação não informados.");
                _logger.LogWarning("Tentativa de criar publicação sem dados.");
                throw new ArgumentException("Os dados da publicação são obrigatórios.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == communityPost.UserId && u.IsActive);

            if (user == null)
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Usuário não encontrado ou inativo.");
                _logger.LogWarning("Usuário {UserId} não encontrado ou inativo ao criar publicação.", communityPost.UserId);
                throw new InvalidOperationException("Usuário não encontrado ou inativo.");
            }


            if (string.IsNullOrWhiteSpace(communityPost.Category))
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Categoria não informada.");
                _logger.LogWarning("Tentativa de criar publicação sem categoria para o usuário {UserId}.", communityPost.UserId);
                throw new ArgumentException("A categoria da publicação é obrigatória.");
            }

            if (string.IsNullOrWhiteSpace(communityPost.Content))
            {
                activity?.SetStatus(ActivityStatusCode.Error,"Conteúdo não informado.");
                _logger.LogWarning("Tentativa de criar publicação sem conteúdo para o usuário {UserId}.", communityPost.UserId);
                throw new ArgumentException("O conteúdo da publicação é obrigatório.");
            }

            try
            {
                _context.CommunityPosts.Add(communityPost);

                if (user is Tutor tutor)
                {
                    tutor.AddEngagementPoints(EngagementPoints.CommunityPost);

                    _logger.LogInformation("Pontos de engajamento adicionados ao tutor {TutorId} pela criação de publicação.",tutor.UserId);
                    
                    activity?.SetTag("user.type","Tutor");
                
                }
                else
                {
                    activity?.SetTag("user.type",user.TypeUser.ToString());
                }

                await _context.SaveChangesAsync();

                activity?.SetTag("community_post.id",communityPost.CommunityPostId);
                activity?.SetTag("community_post.category",communityPost.Category);
                activity?.SetStatus(ActivityStatusCode.Ok);

                _logger.LogInformation("Publicação criada com sucesso. CommunityPostId: {CommunityPostId}, UserId: {UserId}.",communityPost.CommunityPostId,communityPost.UserId);

                _communityPostsCreatedCounter.Add(1);

                return communityPost;
            }

            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error,ex.Message);
                _logger.LogError(ex,"Erro ao criar publicação para o usuário {UserId}.",communityPost.UserId);
                throw;
            }
        
        }

        public async Task DeleteAsync(int id, int authenticatedUserId)
        {
            _logger.LogInformation("Iniciando exclusão da publicação {CommunityPostId} pelo usuário {UserId}.", id,authenticatedUserId);

            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de excluir publicação com ID inválido: {CommunityPostId}.",id);

                throw new ArgumentException("O ID da publicação deve ser maior que zero.");
            }


            var communityPost = await _context.CommunityPosts.FirstOrDefaultAsync(cp => cp.CommunityPostId == id);

            if (communityPost == null)
            {
                _logger.LogWarning("Publicação {CommunityPostId} não encontrada para exclusão.",id);
                throw new InvalidOperationException("Publicação não encontrada.");
            }
                

            if (communityPost.UserId != authenticatedUserId)
            {
                _logger.LogWarning("Usuário {UserId} tentou excluir a publicação {CommunityPostId}, pertencente ao usuário {PostOwnerId}.",authenticatedUserId,id,communityPost.UserId);

                throw new UnauthorizedAccessException("Você não pode excluir esta publicação.");
            }

            try
            {
                _context.CommunityPosts.Remove(communityPost);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Publicação {CommunityPostId} excluída com sucesso pelo usuário {UserId}.", id, authenticatedUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Erro ao excluir publicação {CommunityPostId} pelo usuário {UserId}.", id, authenticatedUserId);
                throw;
            }

        }
    }
}