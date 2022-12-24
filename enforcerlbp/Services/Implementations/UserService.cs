using AutoMapper;
using DataAccess.UnitOfWork;
using DTOs.RequestObject;
using DTOs.ResponseObject;
using Entities.Documents;
using Entities.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
        private readonly IUnitOfWork<PatientTherapist> _unitOfWorkPatientTherapist;

        public UserService(
            IUnitOfWork<Physiotherapist> unitOfWorkPhysio,
            IUnitOfWork<Patient> unitOfWorkPatient,
            IMapper mapper,
            IFirebase firebase,
            IUnitOfWork<VerificationDocument> unitOfWorkVerificationDocument,
            IUnitOfWork<PatientTherapist> unitOfWorkPatientTherapist
            )
        {
            _unitOfWorkPhysio = unitOfWorkPhysio;
            _unitOfWorkPatient = unitOfWorkPatient;
            _mapper = mapper;
            _firebase = firebase;
            _unitOfWorkVerificationDocument = unitOfWorkVerificationDocument;
            _unitOfWorkPatientTherapist = unitOfWorkPatientTherapist;
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
            if(document.DegreeCert.FileName.Length < 1 && document.License.FileName .Length < 1) return new ResponseModel { Status = false, Response = "All required documents must be attached" };
            string[] allowedExtensions = { ".doc", ".docx", ".ppt", ".pdf" };
            string getDegreeCertExtension = Path.GetExtension(document.DegreeCert.FileName);
            string getLicenseExtension = Path.GetExtension(document.License.FileName);
            
            //TODO: refactor the if statement
            if (!allowedExtensions.Contains(getDegreeCertExtension) && !allowedExtensions.Contains(getLicenseExtension))
            {
                return new ResponseModel { Status = false, Response = "File select is not a document or the document is not supported.Ensure you select any of the listed document format: .doc, .docx, .ppt, .pdf" };
            }
            var returnedDegreeUrl = await _firebase.FirebaseFileUpload(document.DegreeCert, "verificationdocs");
            var returnedLicenseUrl = await _firebase.FirebaseFileUpload(document.License, "verificationdocs");
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
          var physioToUpdate = await  _unitOfWorkPhysio.Repository.GetByID(physioId);

            //accept properties to change
            physioToUpdate.FirstName = string.IsNullOrWhiteSpace(therapist.FirstName) ? physioToUpdate.FirstName : therapist.FirstName;
            physioToUpdate.MiddleName = string.IsNullOrWhiteSpace(therapist.MiddleName) ? physioToUpdate.MiddleName : therapist.MiddleName;
            physioToUpdate.LastName = string.IsNullOrWhiteSpace(therapist.LastName) ? physioToUpdate.LastName : therapist.LastName;
            physioToUpdate.About = string.IsNullOrWhiteSpace(therapist.About) ? physioToUpdate.About : therapist.About; 
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

            
            _unitOfWorkPhysio.Repository.Update(physioToUpdate);
           await _unitOfWorkPhysio.Save();
           return new ResponseModel { Status = true, Response = "Update Successful",ReturnObj = physioToUpdate};
          
        }

        public ResponseModel GetPhysiotherapists(int pageNo)
        {
            try
            {
                var physios = _unitOfWorkPhysio.Repository.GetAllQuery()
                .OrderByDescending(x => x.Ratings)
                .Take(pageNo).ToList();

                return new ResponseModel { Status = true, Response = "Success", ReturnObj = physios };
            }
            catch (Exception ex)
            {
                return new ResponseModel { Status = false, Response = $"{ex.Message ?? ex.InnerException.Message}" };
            }
        }

        public async Task<ResponseModel> GetAPhysioTherapist(Guid Id)
        {
            var physio = await _unitOfWorkPhysio.Repository.GetByID(Id);
            var physioDTO = _mapper.Map<PhysiotherapistDTO>(physio);
            return new ResponseModel { Status = true, Response = "successful", ReturnObj = physioDTO };
        }

        public async Task<ResponseModel> Verification(Guid physiotherapistId)
        {
            var physioToVerify = await _unitOfWorkPhysio.Repository.GetByID(physiotherapistId);
            physioToVerify.IsVerified = true;
            _unitOfWorkPhysio.Repository.Update(physioToVerify);
            await _unitOfWorkPhysio.Save();
            return new ResponseModel { Status=true, Response = "Physiotherapist is Verified", ReturnObj = physioToVerify};


        }

        #region PatientPhysiotherapist Connection
        public ResponseModel GetPhysiotherapists(string searchText)
        {
            try
            {
                var physios = _unitOfWorkPhysio.Repository.GetAllQuery()
                .Where(x => (x.FirstName + " " + x.LastName + " " + x.MiddleName).ToLower().Contains(searchText.ToLower()))
                .Select(p=> new PhysioTherapistConnectDTO {ID = p.ID, FirstName = p.FirstName, MiddleName = p.MiddleName, LastName = p.LastName, Speciality = p.Speciality });

                if (physios.Any())
                {
                    return new ResponseModel { Status = true, Response = "Success", ReturnObj = physios.ToList() };
                }
                else
                {
                    return new ResponseModel { Status = true, Response = "Success", ReturnObj = new List<Physiotherapist>() };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel { Status = false, Response = $"{ex.Message ?? ex.InnerException.Message}" };
            }
        }

        //my(patient) physiotherapist
        public ResponseModel MyPhysiotherapists(Guid patientId)
        {
            var connectionQuery = _unitOfWorkPatientTherapist.Repository.GetAllQuery()
                .Where(p => p.PatientID == patientId && p.ConnectionStatus == ConnectionStatus.accepted);
            if (!connectionQuery.Any()) return new ResponseModel { Status = false, Response = "You have not connected to any physiotherapist" };

            var physiosIds = connectionQuery.Select(x => x.PhysiotherapistID).ToList();

            var physios = _unitOfWorkPhysio.Repository.GetAllQuery()
                .Where(x => physiosIds.Contains(x.ID))
                .Select(p => new PhysioTherapistConnectDTO { ID = p.ID, FirstName = p.FirstName, MiddleName = p.MiddleName, LastName = p.LastName, Speciality = p.Speciality });
            if (physios.Any())
            {
                return new ResponseModel {Status = true, Response = "Success", ReturnObj = physios.ToList() };
            }

            return new ResponseModel { Status = false, Response = "Failed" };
        }

        //patient send connection request
        public async Task<ResponseModel> PatientConnectRequest(ConnectionRequestDTO request)
        {
            var isConnected = _unitOfWorkPatientTherapist.Repository.GetAllQuery()
                .FirstOrDefault(x => x.PatientID == request.PatientID && x.PhysiotherapistID == request.PhysiotherapistID);
            if (isConnected != null && isConnected.ConnectionStatus == ConnectionStatus.accepted) return new ResponseModel { Status = false, Response = $"You are already a connection" };
            if (isConnected != null && isConnected.ConnectionStatus == ConnectionStatus.sent) return new ResponseModel { Status = false, Response = $"Your connection request has been sent earlier but has not been accepted. Kindly reach out to the physiotherapist" };
            if (isConnected != null && (isConnected.ConnectionStatus == ConnectionStatus.rejected || isConnected.ConnectionStatus == ConnectionStatus.disconnected))
            {
                request.ConnectionStatus = ConnectionStatus.sent;
                return await PatientConnectStatus(request);
            }

            try
            {
                var patPhy = _mapper.Map<PatientTherapist>(request);
                patPhy.ConnectionStatus = ConnectionStatus.sent;
                await _unitOfWorkPatientTherapist.Repository.Create(patPhy);

                await _unitOfWorkPatientTherapist.Save();

                //TODO: send notification and email here in controller

                return new ResponseModel { Status = true, Response = "Successful" };
            }
            catch (Exception ex)
            {

                return new ResponseModel { Status = false, Response = $"{ex.Message ?? ex.InnerException.Message}" };
            }
        }

        // Accept, reject, or disconnect a patient
        public async Task<ResponseModel> PatientConnectStatus(ConnectionRequestDTO request)
        {
            var isConnected = _unitOfWorkPatientTherapist.Repository.GetAllQuery()
                .FirstOrDefault(x => x.PatientID == request.PatientID && x.PhysiotherapistID == request.PhysiotherapistID);
            if (isConnected == null) return new ResponseModel { Status = false, Response = $"No connection exists between the patient and physiotherapist chosen" };

            try
            {
                isConnected.ConnectionStatus = request.ConnectionStatus;
                _unitOfWorkPatientTherapist.Repository.Update(isConnected);
                await _unitOfWorkPatientTherapist.Save();

                //TODO: sent noification and email here
                
                return new ResponseModel { Status = true, Response = "Updated Succefully", ReturnObj = request};
            }
            catch (Exception ex)
            {
                return new ResponseModel { Status = false, Response = $"{ex.Message ?? ex.InnerException.Message}" };
            }
        }

        #endregion


    }

}
