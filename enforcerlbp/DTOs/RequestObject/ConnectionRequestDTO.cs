using Entities.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.RequestObject
{
    public class ConnectionRequestDTO
    {
        public List<ConnectionsDTO> Pending { get; set; }
        public List<ConnectionsDTO> Current { get; set; }
    }

    public class ConnectionsDTO
    {
        public string PatientName { get; set; }
        public string PatientEmail { get; set; }
        public string PatientPhone { get; set; }
        public int PatientAge { get; set; }
        public string PatientPhoto { get; set; }
        public string PatientGender { get; set; }
        public string TherapistName { get; set; }
        public string TherapistEmail { get; set; }
        public string TherapistPhone { get; set; }
        public int TherapistAge { get; set; }
        public string TherapistPhoto { get; set; }
        public string TherapistGender { get; set; }
        public Guid PatientID { get; set; }
        public Guid PhysiotherapistID { get; set; }
        public ConnectionStatus ConnectionStatus { get; set; }
        public string Message { get; set; }
    }
}
