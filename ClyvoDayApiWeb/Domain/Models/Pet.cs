using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ClyvoDayApiWeb.Domain.Models
{
    public class Pet
    {
        public int PetId { get; private set; }
        public int TutorId { get; private set; }
        public Tutor? Tutor { get; private set; }
        public string Name { get; private set; }
        public string Species { get; private set; }
        public string Breed { get; private set; }
        public string Sex { get; private set; }
        public int Age { get; private set; }
        public DateTime BirthDate { get; private set; }
        public ICollection<PetMonitoring> PetMonitorings { get; private set; }
        public ICollection<CareEvent> CareEvents { get; private set; }
        public ICollection<DailyPetLog> DailyPetLogs { get; private set; }

        protected Pet() {
            PetMonitorings = new List<PetMonitoring>();
            CareEvents = new List<CareEvent>();
            DailyPetLogs = new List <DailyPetLog>();
        }

        public Pet (int tutorId, string name, string species, string breed, string sex, int age, DateTime birthDate)
        {
            TutorId = tutorId;
            Name = name;
            Species = species;
            Breed = breed;
            Sex = sex;
            Age = age;
            BirthDate = birthDate;
            PetMonitorings = new List<PetMonitoring>();
            CareEvents = new List<CareEvent>();
            DailyPetLogs = new List<DailyPetLog>();

        }

    }
}
