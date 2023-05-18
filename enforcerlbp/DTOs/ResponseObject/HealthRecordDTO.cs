using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.ResponseObject
{
    public class HealthRecordDTO
    {
        public int TotalPrescribedExercise { get; set; }
        public int CompletedExercises { get; set; }
        public int OngoingExercises { get; set; }
    }
}
