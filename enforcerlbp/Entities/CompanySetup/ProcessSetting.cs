using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.CompanySetup
{
    public class ProcessSetting
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        // A stringified verification object is stored in details
        public string  Details { get; set; }
        

    }
    public class Verification
    {
        public Guid ID { get; set; }
        public string DocumentName { get; set; }
        public bool IsRequired { get; set; }
        public bool CanExpire { get; set; }
        public decimal YearsDocumentIsActive { get; set; }
    }
}
