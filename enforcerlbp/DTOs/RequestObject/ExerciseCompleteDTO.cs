using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.RequestObject
{
    public class ExerciseCompleteDTO
    {
        public Guid ExercisePrescriptionId { get; set; }
        public IFormFile Video { get; set; }

    }
}
