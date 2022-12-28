using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.RequestObject
{
    public class FeedbackRequestDTO
    {
        public Guid PatientId { get; set; }
        public string FeedbackMessage { get; set; }
        public Guid ExercisePrescriptionId { get; set; }
        public int PainResponse { get; set; }
        public string OtherComplaint { get; set; }
    }

}
