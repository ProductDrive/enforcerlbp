using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public  class Exercise
    {
        public Guid Id{ get; set; }
        public string Name{ get; set; }
        public string Description{ get; set; }
        public string FileUrl { get; set; }
        public string Category { get; set; }
        public bool IsSuggested { get; set; }

        public DateTime DateCreated{ get; set; }


    }
   


}
