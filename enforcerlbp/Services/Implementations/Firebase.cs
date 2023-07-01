using DTOs.RequestObject;
using DTOs.ResponseObject;
using Entities.Documents;
using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
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
        private FirebaseFileStore _firebaseStore { get; set; }

        //public Firebase(IOptions<FirebaseFileStore> firebaseStore)
        //{
        //    _firebaseStore = firebaseStore.Value;
        //}
        public IConfiguration Configuration { get; }
        public Firebase(IConfiguration configuration)
        {
            Configuration = configuration;
            _firebaseStore = new FirebaseFileStore();
            _firebaseStore.ApiKey = Configuration["FirebaseFileStore:ApiKey"];
            _firebaseStore.AuthEmail = Configuration["FirebaseFileStore:AuthEmail"];
            _firebaseStore.AuthPassword = Configuration["FirebaseFileStore:AuthPassword"];
            _firebaseStore.Bucket = Configuration["FirebaseFileStore:Bucket"];
            
        }

        public async Task <string> FirebaseFileUpload(IFormFile file, string folderName)
        {
            string getExtension = Path.GetExtension(file.FileName);
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
                        .Child(folderName.Split('|')[0])
                        .Child(file.FileName + folderName.Split('|')[1])

                        .PutAsync(ms, cancellation.Token);

                    return  uploadedFileUrl;
                }
                catch (Exception ex)
                {
                    Log.Fatal($"Error occured while uploading exercise to firbase.>>>>>{ex.InnerException.Message ?? ex.Message}");
                    return "";

                }


            }
            return "";
        }

        
    }
}
