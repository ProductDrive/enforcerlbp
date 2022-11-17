using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Consultation
{
    public class ConsultationService
    {
       public Guid ID { get; set; }
       public string Name { get; set; }
       public Guid PhysiotherapistID { get; set; }
       public decimal AmountByTime { get; set; }
       public bool IsAvailable { get; set; }

    }
}
