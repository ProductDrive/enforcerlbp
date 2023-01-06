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
        private DateTime therapitDOB;
        public int Age => DateTime.TryParse(DOB, out therapitDOB) ? ((int)(DateTime.Now - therapitDOB).TotalDays / 365) : 0;
        public string Gender { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateLastModified { get; set; }
    }
}
