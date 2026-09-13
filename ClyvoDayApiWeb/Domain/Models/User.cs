using ClyvoDayApiWeb.Domain.Enums;
using System.Text.Json.Serialization;

namespace ClyvoDayApiWeb.Domain.Models
{
    public abstract class User
    {
        public int UserId { get; protected set; }
        public string FullName { get; protected set; }
        public string Email { get; protected set; }
        [JsonIgnore]
        public string PasswordHash { get; protected set; }
        public string PhoneNumber { get; protected set; }
        public EnumTypeUser TypeUser { get; protected set; }
        public bool IsActive { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public ICollection<CommunityPost> CommunityPosts { get; private set; }

        protected User() {
            CommunityPosts = new List<CommunityPost>();
        }

        protected User(string fullName, string email, string password, string phoneNumber)
        {
            FullName = fullName;
            Email = email;
            PasswordHash = password;
            PhoneNumber = phoneNumber;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            CommunityPosts = new List<CommunityPost>();

        }


        public void UpdateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("O e-mail é obrigatório.");

            Email = email.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePasswordHash(string passwordHash)
        {
            PasswordHash = passwordHash;
        }


        public void UpdatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("O número de telefone é obrigatório.");

            PhoneNumber = phoneNumber.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

    }
}
