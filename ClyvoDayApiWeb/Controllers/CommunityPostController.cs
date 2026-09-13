using ClyvoDayApiWeb.Domain.Models;
using ClyvoDayApiWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace ClyvoDayApiWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommunityPostController : ControllerBase
    {
        private readonly CommunityPostService _communityPostService;

        public CommunityPostController(CommunityPostService communityPostService)
        {
            _communityPostService = communityPostService;
        }

        
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var posts = await _communityPostService.GetAllAsync();

            var response = posts.Select(post => new
            {
                communityPostId = post.CommunityPostId,
                userId = post.UserId,
                userName = post.User?.FullName,
                userType = post.User?.TypeUser.ToString(),
                category = post.Category,
                content = post.Content,
                imageUrl = post.ImageUrl,
                location = post.Location,
                registeredAt = post.RegisteredAt
            });

            return Ok(response);
        }


        
        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                var post = await _communityPostService.GetByIdAsync(id);

                if (post == null)
                    return NotFound("Publicação não encontrada.");

                return Ok(new
                {
                    communityPostId = post.CommunityPostId,
                    userId = post.UserId,
                    userName = post.User?.FullName,
                    userType = post.User?.TypeUser.ToString(),
                    category = post.Category,
                    content = post.Content,
                    imageUrl = post.ImageUrl,
                    location = post.Location,
                    registeredAt = post.RegisteredAt
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        
        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<List<CommunityPost>>> GetByUserId(int userId)
        {
            try
            {
                var posts = await _communityPostService.GetByUserIdAsync(userId);

                return Ok(posts);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        
        [HttpPost]
        public async Task<ActionResult<CommunityPost>> Create([FromBody] JsonElement body)
        {
            try
            {
                var userId = GetAuthenticatedUserId();

                var category = body.GetProperty("category").GetString();

                var content = body.GetProperty("content").GetString();

                string? imageUrl = null;

                if (body.TryGetProperty("imageUrl", out var imageUrlProperty))
                {
                    imageUrl = imageUrlProperty.GetString();
                }

                string? location = null;

                if (body.TryGetProperty("location", out var locationProperty))
                {
                    location = locationProperty.GetString();
                }

                var communityPost = new CommunityPost(userId, category ?? string.Empty, content ?? string.Empty, imageUrl, location);

                var created = await _communityPostService.CreateAsync(communityPost);

                return CreatedAtAction(nameof(GetById), new { id = created.CommunityPostId }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var userId = GetAuthenticatedUserId();

                await _communityPostService.DeleteAsync(id, userId);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Usuário não autenticado.");

            return userId;
        }
    }
}
