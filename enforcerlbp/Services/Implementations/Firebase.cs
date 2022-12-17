using DTOs.RequestObject;
using DTOs.ResponseObject;
using Entities.Documents;
using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class Firebase : IFirebase
    {
        //private readonly IOptions<FirebaseFileStore> _firebaseStore;
        private readonly FirebaseFileStore _firebaseStore;

        public Firebase(IOptions<FirebaseFileStore> firebaseStore)
        {
            _firebaseStore = firebaseStore.Value;
        }

        public async Task <string> FirebaseFileUpload(IFormFile file, string folderName,  Guid? physiotherapistId)
        {
            //string[] allowedExtensions = { ".doc", ".docx", ".ppt", ".pdf" };
            string getExtension = Path.GetExtension(file.FileName);
            //if (!allowedExtensions.Contains(getExtension))
            //{
            //    return new ResponseModel { Status = false, Response = "Success", ReturnObj = "File select is not a document or the document is not supported.Ensure you select any of the listed document format: .doc, .docx, .ppt, .pdf" };
            //}

            if (file.Length > 0)
            {

                try
                {

                    Stream ms = file.OpenReadStream();

                    var auth = new FirebaseAuthProvider(new FirebaseConfig(_firebaseStore.ApiKey));
                    var authLink = await auth.SignInWithEmailAndPasswordAsync(_firebaseStore.AuthEmail, _firebaseStore.AuthPassword);

                    // you can use CancellationTokenSource to cancel the upload midway
                    var cancellation = new CancellationTokenSource();

                    string uploadedFileUrl = await new FirebaseStorage(
                        _firebaseStore.Bucket,
                        new FirebaseStorageOptions
                        {
                            AuthTokenAsyncFactory = () => Task.FromResult(authLink.FirebaseToken),
                            ThrowOnCancel = true // when you cancel the upload, exception is thrown. By default no exception is thrown
                        })
                        .Child(folderName)
                        .Child(file.FileName)

                        .PutAsync(ms, cancellation.Token);

                    return  uploadedFileUrl;
                }
                catch (Exception)
                {
                    return "";

                }


            }
            return "";
        }

        
    }
}
