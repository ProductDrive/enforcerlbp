using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.RequestObject
{
    public class ExerciseDTO
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FileUrl { get; set; }
        public string Category { get; set; }
        public bool IsSuggested { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;
    }

}
