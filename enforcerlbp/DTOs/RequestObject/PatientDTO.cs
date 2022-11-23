using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.RequestObject
{
    public class PatientDTO
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

        public string Gender { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateLastModified { get; set; }
    }
}
