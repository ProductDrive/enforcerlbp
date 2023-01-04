using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.RequestObject
{
    public class PhysioSessionDTO
    {
        public Guid ID { get; set; }
        public Guid PhysiotherapistID { get; set; }
        public string Name { get; set; }
        public decimal AmountBySession { get; set; }
        public bool IsAvailable { get; set; }
    }
}
