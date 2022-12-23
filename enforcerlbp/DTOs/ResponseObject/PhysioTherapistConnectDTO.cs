using Entities.Consultation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.ResponseObject
{
    public class PhysioTherapistConnectDTO
    {
        public Guid ID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {MiddleName??""} {LastName}";
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string Addressline { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string DOB { get; set; }
        public int Age { get; set; }
        public int Experience { get; set; }
        public decimal Ratings { get; set; }
        public string About { get; set; }
        public string Speciality { get; set; }
        public string Gender { get; set; }
        public List<ConsultationService> ConsultationService { get; set; }
        public List<PhysioSession> PhysioSession { get; set; }
        public bool IsVerified { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateLastModified { get; set; }
        public bool IsOnboarded { get; set; }
    }
}
