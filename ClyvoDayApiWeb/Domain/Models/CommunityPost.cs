using ClyvoDayApiWeb.Domain.Enums;

namespace ClyvoDayApiWeb.Domain.Models
{
    public class CommunityPost
    {
        public int CommunityPostId { get; private set; }
        public int UserId {  get; private set; }
        public User? User { get; private set; }
        public string Category { get; private set; }
        public string Content { get; private set; }
        public string? ImageUrl { get; private set; }
        public string? Location { get; private set; }
        public DateTime RegisteredAt { get; private set; }
        

        protected CommunityPost() {}

        public CommunityPost(int userId, string category, string content, string? imageUrl, string? location)
        {
            UserId = userId;
            Category = category;
            Content = content;
            ImageUrl = imageUrl;
            Location = location;
            RegisteredAt = DateTime.UtcNow;
        }
    }
}
