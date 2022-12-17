using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.RequestObject
{
    public class FileDTO
    {
        public Guid PhysiotherapistId { get; set; }
       
        public string NameOfDocument { get; set; }
        public IFormFile DegreeCert { get; set; }
        public IFormFile License { get; set; }

    }
}
