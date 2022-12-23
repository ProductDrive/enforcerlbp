using DTOs.ResponseObject;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IFirebase
    {
        Task<string> FirebaseFileUpload(IFormFile file, string folderName);
        
    }
}
