using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class Notification
    {
        public Guid ID { get; set; }
        public Guid OwnerId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
    }
}
