using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Documents
{
    public class VerificationDocument
    {
        public Guid ID { get; set; }
        public Guid PhysiotherapistID { get; set; }
        public string NameOfDocument{ get; set; }
        public string DocumentUrl { get; set; }



    }
}
