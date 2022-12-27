using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.RequestObject
{
    public class ExercisePrescriptionDTO
    {

        public Guid ID { get; set; }
        public Guid ExerciseId { get; set; }
        public string ExerciseName { get; set; }
        public Guid PatientId { get; set; }
        public Guid PhysiotherapistId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCompleted { get; set; }
        public string Repetitions { get; set; }
        public string Set { get; set; }
        public string Hold { get; set; }
        public string Time { get; set; }
        public string Description { get; set; }
        public string ExerciseSummary => $"{ExerciseName},-{Repetitions} repetitions, -{Time}, to be completed {EndDate.ToString("d")}";
        public string SubmittedVideoUrl { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

       
    }
}
