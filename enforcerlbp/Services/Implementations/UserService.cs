using AutoMapper;
using DataAccess.UnitOfWork;
using DTOs.RequestObject;
using DTOs.ResponseObject;
using Entities.Documents;
using Entities.Users;
using Microsoft.AspNetCore.Http;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork<Physiotherapist> _unitOfWorkPhysio;
        private readonly IUnitOfWork<Patient> _unitOfWorkPatient;
        private readonly IMapper _mapper;
        private readonly IFirebase _firebase;
        private readonly IUnitOfWork<VerificationDocument> _unitOfWorkVerificationDocument;

        public UserService(
            IUnitOfWork<Physiotherapist> unitOfWorkPhysio,
            IUnitOfWork<Patient> unitOfWorkPatient,
            IMapper mapper,
            IFirebase firebase,
            IUnitOfWork<VerificationDocument> unitOfWorkVerificationDocument
            )
        {
            _unitOfWorkPhysio = unitOfWorkPhysio;
            _unitOfWorkPatient = unitOfWorkPatient;
            _mapper = mapper;
            _firebase = firebase;
            _unitOfWorkVerificationDocument = unitOfWorkVerificationDocument;
        }

        public async Task<ResponseModel> CreatePhysiotherapist(PhysiotherapistDTO model)
        {
            var physiotherapist = _mapper.Map<PhysiotherapistDTO, Physiotherapist>(model);
            await _unitOfWorkPhysio.Repository.Create(physiotherapist);
            await _unitOfWorkPhysio.Save();
            return new ResponseModel{ Status=true,Response="Therapist created Successfully"};
        }
        public async Task<ResponseModel> CreatePatient(PatientDTO model)
        {
            var patient = _mapper.Map<PatientDTO, Patient>(model);
            await _unitOfWorkPatient.Repository.Create(patient);
            await _unitOfWorkPatient.Save();
            return new ResponseModel { Status = true, Response = "Patient created Successfully" };
        }

        public async Task<ResponseModel> PhysiotherapistVerificationFilesUpload(FileDTO document)
        {
            // 1. ensure files are not empty - done
            // 2. ensure file types are acceptable -done
            // 3. upload each file and get their urls
            //4. create verificatio documents for each files
            // 5. set isOnboarded to true

            if(document.DegreeCert.FileName.Length < 1 && document.License.FileName .Length < 1) return new ResponseModel { Status = false, Response = "All required documents must be attached" };
            string[] allowedExtensions = { ".doc", ".docx", ".ppt", ".pdf" };
            string getDegreeCertExtension = Path.GetExtension(document.DegreeCert.FileName);
            string getLicenseExtension = Path.GetExtension(document.License.FileName);
            
            //TODO: refactor the if statement
            if (!allowedExtensions.Contains(getDegreeCertExtension) && !allowedExtensions.Contains(getLicenseExtension))
            {
                return new ResponseModel { Status = false, Response = "File select is not a document or the document is not supported.Ensure you select any of the listed document format: .doc, .docx, .ppt, .pdf" };
                
            }

            

            var returnedDegreeUrl = await _firebase.FirebaseFileUpload(document.DegreeCert, "verificationdocs", document.PhysiotherapistId);
            var returnedLicenseUrl = await _firebase.FirebaseFileUpload(document.License, "verificationdocs", document.PhysiotherapistId);
            var Documents = new List<VerificationDocument>()
            {
                new VerificationDocument
                {
                    PhysiotherapistID = document.PhysiotherapistId,
                    NameOfDocument = document.NameOfDocument,
                    DocumentUrl = returnedDegreeUrl

                },
                new VerificationDocument
                {
                    PhysiotherapistID = document.PhysiotherapistId,
                    NameOfDocument = document.NameOfDocument,
                    DocumentUrl = returnedLicenseUrl

                }

            };
            try 
            {
                await _unitOfWorkVerificationDocument.Repository.CreateMany(Documents);
                await UpdatePhysiotherapist(document.PhysiotherapistId, new Physiotherapist{ IsOnboarded = true, ID = document.PhysiotherapistId});

                await _unitOfWorkVerificationDocument.Save();
                return new ResponseModel { Status = true, Response = "Documents uploaded successfully" };

            }
            catch(Exception)
            {
                return new ResponseModel { Status = true, Response = "Somthing went wrong while saving the documents" };
            }
        }

        public async Task<ResponseModel> UpdatePhysiotherapist(Guid physioId, Physiotherapist therapist)
        {
            // 1. retrieve the therapist from the database
          var physioToUpdate = await  _unitOfWorkPhysio.Repository.GetByID(physioId);

            // 2. accept properties to change
            physioToUpdate.FirstName = string.IsNullOrWhiteSpace(therapist.FirstName) ? physioToUpdate.FirstName : therapist.FirstName;
            physioToUpdate.MiddleName = string.IsNullOrWhiteSpace(therapist.MiddleName) ? physioToUpdate.MiddleName : therapist.MiddleName;
            physioToUpdate.LastName = string.IsNullOrWhiteSpace(therapist.LastName) ? physioToUpdate.LastName : therapist.LastName;
            physioToUpdate.PhysioSessionID = therapist.PhysioSessionID != Guid.Empty?therapist.PhysioSessionID : physioToUpdate.PhysioSessionID;
            physioToUpdate.About = string.IsNullOrWhiteSpace(therapist.About) ? physioToUpdate.About : therapist.About; 
            //physioToUpdate.DateCreated = physioToUpdate.DateCreated;
            physioToUpdate.DateLastModified = DateTime.UtcNow;
            physioToUpdate.Age = therapist.Age == 0 ? physioToUpdate.Age : therapist.Age;
            physioToUpdate.IsVerified = therapist.IsVerified == false ? physioToUpdate.IsVerified : therapist.IsVerified;
            physioToUpdate.Ratings = therapist.Ratings == 0 ? physioToUpdate.Ratings : therapist.Ratings;
            physioToUpdate.DOB = therapist.DOB;
            physioToUpdate.Addressline = string.IsNullOrWhiteSpace(therapist.Addressline) ? physioToUpdate.Addressline : therapist.Addressline;
            physioToUpdate.PhoneNumber = string.IsNullOrWhiteSpace(therapist.PhoneNumber) ? physioToUpdate.PhoneNumber : therapist.PhoneNumber;
            physioToUpdate.Country = string.IsNullOrWhiteSpace(therapist.Country) ? physioToUpdate.Country : therapist.Country;
            physioToUpdate.ProfilePictureUrl = string.IsNullOrWhiteSpace(therapist.ProfilePictureUrl) ? physioToUpdate.ProfilePictureUrl : therapist.ProfilePictureUrl;
            physioToUpdate.Gender = string.IsNullOrWhiteSpace(therapist.Gender) ? physioToUpdate.Gender : therapist.Gender;
            physioToUpdate.Email = string.IsNullOrWhiteSpace(therapist.Email) ? physioToUpdate.Email : therapist.Email;
            physioToUpdate.Experience = therapist.Experience == 0 ? physioToUpdate.Experience : therapist.Experience;
            physioToUpdate.State = string.IsNullOrWhiteSpace(therapist.State) ? physioToUpdate.State : therapist.State;
            physioToUpdate.ConsultationServiceID = therapist.ConsultationServiceID != Guid.Empty ? therapist.ConsultationServiceID : physioToUpdate.ConsultationServiceID;

            // 3. update the entity
            _unitOfWorkPhysio.Repository.Update(physioToUpdate);
            // 4. save changes
           await _unitOfWorkPhysio.Save();
           return new ResponseModel { Status = true, Response = "Update Successful",ReturnObj = physioToUpdate};
            //dates, age, isverified, ratings
        }


        public async Task<ResponseModel> Verification(Guid physiotherapistId, Physiotherapist physio)
        {
            var physioToVerify = await _unitOfWorkPhysio.Repository.GetByID(physiotherapistId);
            physioToVerify.IsVerified = true;
            _unitOfWorkPhysio.Repository.Update(physioToVerify);
            await _unitOfWorkPhysio.Save();
            return new ResponseModel { Status=true, Response = "Physiotherapist is Verified", ReturnObj = physioToVerify};


        }

    }
    
}
