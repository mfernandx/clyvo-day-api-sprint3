using ClyvoDayApiWeb.Domain.Enums;

namespace ClyvoDayApiWeb.Domain.Models
{
    public class CareEvent
    {
        public int CareEventId { get; private set; }
        public int PetId { get; private set; }
        public Pet? Pet { get; private set; }
        public string TypeEvent { get; private set; }
        public string Description { get; private set; }
        public DateTime EventDate { get; private set; }
        public string? Observations { get; private set; }
        public EnumCareEventStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        protected CareEvent() { }

        public CareEvent (int petId, string typeEvent, string description, DateTime eventDate, string? observations)
        { 
            PetId = petId;
            TypeEvent = typeEvent;
            Description = description;
            EventDate = eventDate;
            Observations = observations;
            Status = EnumCareEventStatus.Scheduled;
            CreatedAt = DateTime.UtcNow;
        }

        public void Complete()
        {
            if (Status == EnumCareEventStatus.Completed)
                throw new InvalidOperationException("O evento já foi concluído.");

            if (Status == EnumCareEventStatus.Cancelled)
                throw new InvalidOperationException("Um evento cancelado não pode ser concluído.");

            Status = EnumCareEventStatus.Completed;
            
        }

        public void Cancel()
        {
            if (Status == EnumCareEventStatus.Completed)
                throw new InvalidOperationException("Um evento concluído não pode ser cancelado.");

            Status = EnumCareEventStatus.Cancelled;
        }
    }
}
