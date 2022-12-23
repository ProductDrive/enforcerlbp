using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.RequestObject
{
    public class NotificationRequester
    {
        public Guid OwnerID { get; set; }
        public string Message { get; set; }
    }
}
