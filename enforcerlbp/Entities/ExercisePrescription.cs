using Entities.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class ExercisePrescription
    {
        public Guid Id { get; set; }
        public Guid ExerciseId { get; set; }
        public Exercise Exercise { get; set; }


        public Guid PatientId { get; set; }
        public Patient Patient { get; set; }
        public Guid PhysiotherapistId { get; set; }
        public Physiotherapist Physiotherapist { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Repetitions { get; set; }
        public string Set { get; set; }
        public string Hold { get; set; }
        public string Time { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

    }
}
