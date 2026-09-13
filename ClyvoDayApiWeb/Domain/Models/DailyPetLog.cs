using ClyvoDayApiWeb.Domain.Enums;

namespace ClyvoDayApiWeb.Domain.Models
{
    public class DailyPetLog
    {
        public int DailyPetLogId { get; private set; }
        public int PetId { get; private set; }
        public Pet? Pet { get; private set; }
        public int CreatedByUserId { get; private set; }
        public string DailyPetLogType { get; protected set; }
        public string Content { get; protected set; }
        public string? ImageUrl { get; private set; }
        public EnumPrivacy Privacy { get; protected set; }
        public DateTime RegisteredAt { get; protected set; }
        

        protected DailyPetLog() { }

        public DailyPetLog(int petId, int createdByUserId, string dailyPetLogType, string content, string imageUrl, EnumPrivacy privacy)
        {
            PetId = petId;
            CreatedByUserId = createdByUserId;
            DailyPetLogType = dailyPetLogType;
            Content = content;
            ImageUrl = imageUrl;
            Privacy = privacy;
            RegisteredAt = DateTime.UtcNow;
        }
    }
}
