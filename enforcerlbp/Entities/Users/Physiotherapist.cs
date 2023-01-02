using System;
using Entities.Documents;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using Entities.Consultation;

namespace Entities.Users
{
    public class Physiotherapist
    {
        public Guid ID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string Addressline { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string DOB { get; set; }
        public int Age { get; set; }
        public int Experience { get; set; }
        public double Ratings { get; set; }
        public string RatingData { get; set; }
        public string About { get; set; }
        public string Speciality { get; set; }
        public string Gender { get; set; }
        public ICollection<ConsultationService> ConsultationServices  { get; set; }
        public ICollection<PhysioSession> PhysioSessions { get; set; }
        public bool IsVerified { get; set; }
        public ICollection<VerificationDocument> VerificationDocuments { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateLastModified { get; set; }
        public bool IsOnboarded { get; set; }


    }
}
