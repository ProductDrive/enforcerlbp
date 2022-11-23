using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.ResponseObject
{
    public class ResponseModel
    {
        public string Response { get; set; }
        public bool Status { get; set; }
        public object ReturnObj { get; set; }
        public List<string> Errors { get; set; }
    }
}
