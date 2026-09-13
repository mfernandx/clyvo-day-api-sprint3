using ClyvoDayApiWeb.Domain.Enums;
using System.Reflection.Metadata;
using System.Runtime.ConstrainedExecution;

namespace ClyvoDayApiWeb.Domain.Models
{
    public class Tutor : User
    {
        public int ScoreEngagement { get; private set; }
        public ICollection<Pet> Pets { get; private set; }
        public EnumAchievement Achievement { get; private set; }

        protected Tutor()
        {
            Pets = new List<Pet>();
        }

        public Tutor(string fullName,string email,string passwordHash,string phoneNumber) : base(fullName, email, passwordHash, phoneNumber)
        { 
            ScoreEngagement = 0;
            Achievement = EnumAchievement.Nenhum;
            Pets = new List<Pet>();
            TypeUser = EnumTypeUser.Tutor;
        }

        public void AddEngagementPoints(int points)
        {
            if (points <= 0)
                throw new ArgumentException("A pontuação deve ser maior que zero.");

            ScoreEngagement += points;

            UpdateAchievement();
        }

        private void UpdateAchievement()
        {
            if (ScoreEngagement >= 300)
            {
                Achievement = EnumAchievement.ClyvoMaster;
            }
            else if (ScoreEngagement >= 150)
            {
                Achievement = EnumAchievement.GuardiãoPet;
            }
            else if (ScoreEngagement >= 100)
            {
                Achievement = EnumAchievement.TutorDedicado;
            }
            else if (ScoreEngagement >= 50)
            {
                Achievement = EnumAchievement.InicianteAtencioso;
            }
            else
            {
                Achievement = EnumAchievement.Nenhum;
            }
        }



    }


}

