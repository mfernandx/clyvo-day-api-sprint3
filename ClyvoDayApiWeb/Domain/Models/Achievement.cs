namespace ClyvoDayApiWeb.Domain.Models
{
    public class Achievement
    {
        public int AchievementsId { get; private set; }
        public int TutorId { get; private set; }
        public Tutor? Tutor { get; private set; }
        public string AchievementName { get; private set; }

        public DateTime RegisteredAt { get; private set; }

        protected Achievement () { }

        public Achievement(int tutorId, string achievementName, DateTime registeredAt)
        {
            TutorId = tutorId;
            AchievementName = achievementName;
            RegisteredAt = registeredAt;
        }
    }

   
}
