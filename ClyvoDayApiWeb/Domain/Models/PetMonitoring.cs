namespace ClyvoDayApiWeb.Domain.Models
{
    public class PetMonitoring
    {
        public int PetMonitoringId { get; private set; }
        public int PetId { get; private set; }
        public Pet? Pet { get; private set; }
        public string? Mood { get; private set; }
        public string? EnergyLevel { get; private set; }
        public string? HydrationLevel { get; private set; }
        public string? Food { get; private set; }
        public string? SleepQuality { get; private set; }
        public string? RecentActivities { get; private set; }
        public string? Sociability { get; private set; }
        public bool? TookMedication { get; private set; }
        public decimal? Weight { get; private set; }
        public string? Observations { get; private set; }
        public DateTime RegisteredAt { get; private set; }

        protected PetMonitoring() { }

        public PetMonitoring (int petId, string? mood, string? energyLevel, string? hydrationLevel, string? food, string sleepQuality, string? recentActivities, string? sociability, bool? tookMedication, decimal? weight, string? observations)
        {
            PetId = petId;
            Mood = mood;
            EnergyLevel = energyLevel;
            HydrationLevel = hydrationLevel;
            Food = food;
            SleepQuality = sleepQuality;
            RecentActivities = recentActivities;
            Sociability  = sociability;
            TookMedication = tookMedication;
            Weight = weight;
            Observations = observations;
            RegisteredAt = DateTime.UtcNow;
        }
    }
}
