using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Users
{
    public class PatientTherapist
    {
        public Guid ID { get; set; }
        public Guid PatientID { get; set; }
        public Guid PhysiotherapistID { get; set; }
        public ConnectionStatus ConnectionStatus { get; set; }
    }

    public enum ConnectionStatus
    {
        sent,
        accepted, 
        rejected, 
        disconnected
    }

}
