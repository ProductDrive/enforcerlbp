using Entities.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.RequestObject
{
    public class ConnectionRequestDTO
    {
        public string PatientName { get; set; }
        public string TherapistName { get; set; }
        public Guid PatientID { get; set; }
        public Guid PhysiotherapistID { get; set; }
        public ConnectionStatus ConnectionStatus { get; set; }
        public string Message { get; set; }
    }
}
