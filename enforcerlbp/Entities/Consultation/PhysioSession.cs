using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Consultation
{
    public class PhysioSession
    {
       public Guid ID { get; set; }
       public Guid PhysiotherapistID { get; set; }
       public string Name { get; set; }
       public decimal AmountBySession { get; set; }
       public bool IsAvailable { get; set; }

    }
}
