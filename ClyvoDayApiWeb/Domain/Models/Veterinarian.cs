using ClyvoDayApiWeb.Domain.Enums;

namespace ClyvoDayApiWeb.Domain.Models
{
    public class Veterinarian : User
    {
        public string Crmv { get; private set; }
        public string State { get; private set; }
        public string Specialty { get; private set; }
        

        protected Veterinarian(){}

        public Veterinarian(string fullName,string email,string passwordHash,string phoneNumber, string crmv, string state, string specialty) : base(fullName, email, passwordHash, phoneNumber)
        {
            Crmv = crmv;
            State = state;
            Specialty = specialty;
            TypeUser = EnumTypeUser.Veterinario;
        }

        public void UpdateSpecialty(string newSpecialty)
        {
            Specialty = newSpecialty;
        }


    }
}
