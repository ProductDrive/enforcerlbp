using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.RequestObject
{
    public class FirebaseFileStore
    {
        public string ApiKey { get; set; }
        public string AuthEmail { get; set; }
        public string AuthPassword { get; set; }
        public string Bucket { get; set; }
    }
}
