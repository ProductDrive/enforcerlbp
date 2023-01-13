using AutoMapper;
using DataAccess.UnitOfWork;
using DTOs.RequestObject;
using DTOs.ResponseObject;
using Entities;
using Entities.Consultation;
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
        private readonly IUnitOfWork<ConsultationService> _unitOfWorkConsultation;
        private readonly IUnitOfWork<PhysioSession> _unitOfWorkPhysioSessions;
        private readonly IUnitOfWork<Notification> _unitOfWorkNotifications;

        public UserService(
            IUnitOfWork<Physiotherapist> unitOfWorkPhysio,
            IUnitOfWork<Patient> unitOfWorkPatient,
            IMapper mapper,
            IFirebase firebase,
            IUnitOfWork<VerificationDocument> unitOfWorkVerificationDocument,
            IUnitOfWork<PatientTherapist> unitOfWorkPatientTherapist,
            IUnitOfWork<ConsultationService> unitOfWorkConsultation,
            IUnitOfWork<PhysioSession> unitOfWorkPhysioSessions,
            IUnitOfWork<Notification> unitOfWorkNotifications
            
            )
        {
            _unitOfWorkPhysio = unitOfWorkPhysio;
            _unitOfWorkPatient = unitOfWorkPatient;
            _mapper = mapper;
            _firebase = firebase;
            _unitOfWorkVerificationDocument = unitOfWorkVerificationDocument;
            _unitOfWorkPatientTherapist = unitOfWorkPatientTherapist;
            _unitOfWorkConsultation = unitOfWorkConsultation;
            _unitOfWorkPhysioSessions = unitOfWorkPhysioSessions;
            _unitOfWorkNotifications = unitOfWorkNotifications;
        }

       


        #region Patients
        public async Task<ResponseModel> CreatePatient(PatientDTO model)
        {
            var patient = _mapper.Map<PatientDTO, Patient>(model);
            await _unitOfWorkPatient.Repository.Create(patient);
            await _unitOfWorkPatient.Save();
            return new ResponseModel { Status = true, Response = "Patient created Successfully" };
        }

        public async Task<ResponseModel> GetAPatient(Guid Id)
        {
            var pat = await _unitOfWorkPatient.Repository.GetByID(Id);
            var patDTO = _mapper.Map<PatientDTO>(pat);
            return new ResponseModel { Status = true, Response = "successful", ReturnObj = patDTO };
        }



        #endregion


        #region Physiotherapists
        public ResponseModel GetMyPatients(Guid therapistId)
        {
            var connectionQuery = _unitOfWorkPatientTherapist.Repository.GetAllQuery()
                .Where(p => p.PhysiotherapistID == therapistId && p.ConnectionStatus == ConnectionStatus.accepted);
            if (!connectionQuery.Any()) return new ResponseModel { Status = false, Response = "You have not connected to any physiotherapist" };

            var patientIds = connectionQuery.Select(x => x.PatientID).ToList();

            var patientss = _unitOfWorkPatient.Repository.GetAllQuery()
                .Where(x => patientIds.Contains(x.ID))
                .Select(p => new PatientDTO { ID = p.ID, FirstName = p.FirstName, MiddleName = p.MiddleName, LastName = p.LastName, Email = p.Email, PhoneNumber = p.PhoneNumber });
            if (patientss.Any())
            {
                return new ResponseModel { Status = true, Response = "Success", ReturnObj = patientss.ToList() };
            }

            return new ResponseModel { Status = false, Response = "Failed" };
        }
        public ResponseModel GetMyPhysiotherapist(Guid patientId)
        {
            var connectionQuery = _unitOfWorkPatientTherapist.Repository.GetAllQuery()
                .Where(p => p.PatientID == patientId && p.ConnectionStatus == ConnectionStatus.accepted);
            if (!connectionQuery.Any()) return new ResponseModel { Status = false, Response = "You have not connected to any physiotherapist" };

            var therapistIds = connectionQuery.Select(x => x.PhysiotherapistID).ToList();

            var therapists = _unitOfWorkPhysio.Repository.GetAllQuery()
                .Where(x => therapistIds.Contains(x.ID))
                .Select(p => new PhysiotherapistDTO { ID = p.ID, FirstName = p.FirstName, MiddleName = p.MiddleName, LastName = p.LastName, Email = p.Email, PhoneNumber = p.PhoneNumber });
            if (therapists.Any())
            {
                return new ResponseModel { Status = true, Response = "Success", ReturnObj = therapists.ToList() };
            }

            return new ResponseModel { Status = false, Response = "Failed" };
        }

        public async Task<ResponseModel> CreatePhysiotherapist(PhysiotherapistDTO model)
        {
            var physiotherapist = _mapper.Map<PhysiotherapistDTO, Physiotherapist>(model);
            await _unitOfWorkPhysio.Repository.Create(physiotherapist);
            await _unitOfWorkPhysio.Save();
            return new ResponseModel { Status = true, Response = "Therapist created Successfully" };
        }

        public async Task<ResponseModel> PhysiotherapistVerificationFilesUpload(FileDTO document)
        {
            if (document.DegreeCert.FileName.Length < 1 && document.License.FileName.Length < 1) return new ResponseModel { Status = false, Response = "All required documents must be attached" };
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
                await UpdatePhysiotherapist(document.PhysiotherapistId, new PhysiotherapistDTO { IsOnboarded = true, ID = document.PhysiotherapistId });

                await _unitOfWorkVerificationDocument.Save();
                return new ResponseModel { Status = true, Response = "Documents uploaded successfully" };

            }
            catch (Exception)
            {
                return new ResponseModel { Status = true, Response = "Somthing went wrong while saving the documents" };
            }
        }

        public async Task<ResponseModel> UpdatePhysiotherapist(Guid physioId, PhysiotherapistDTO therapist)
        {
            var physioToUpdate = await _unitOfWorkPhysio.Repository.GetByID(physioId);

            //accept properties to change
            physioToUpdate.FirstName = string.IsNullOrWhiteSpace(therapist.FirstName) ? physioToUpdate.FirstName : therapist.FirstName;
            physioToUpdate.MiddleName = string.IsNullOrWhiteSpace(therapist.MiddleName) ? physioToUpdate.MiddleName : therapist.MiddleName;
            physioToUpdate.LastName = string.IsNullOrWhiteSpace(therapist.LastName) ? physioToUpdate.LastName : therapist.LastName;
            physioToUpdate.About = string.IsNullOrWhiteSpace(therapist.About) ? physioToUpdate.About : therapist.About;
            physioToUpdate.DateLastModified = DateTime.UtcNow;
            physioToUpdate.IsVerified = therapist.IsVerified == false ? physioToUpdate.IsVerified : therapist.IsVerified;
            physioToUpdate.Addressline = string.IsNullOrWhiteSpace(therapist.Addressline) ? physioToUpdate.Addressline : therapist.Addressline;
            physioToUpdate.PhoneNumber = string.IsNullOrWhiteSpace(therapist.PhoneNumber) ? physioToUpdate.PhoneNumber : therapist.PhoneNumber;
            physioToUpdate.Country = string.IsNullOrWhiteSpace(therapist.Country) ? physioToUpdate.Country : therapist.Country;
            physioToUpdate.ProfilePictureUrl = string.IsNullOrWhiteSpace(therapist.ProfilePictureUrl) ? physioToUpdate.ProfilePictureUrl : therapist.ProfilePictureUrl;
            physioToUpdate.Gender = string.IsNullOrWhiteSpace(therapist.Gender) ? physioToUpdate.Gender : therapist.Gender;
            physioToUpdate.Email = string.IsNullOrWhiteSpace(therapist.Email) ? physioToUpdate.Email : therapist.Email;
            physioToUpdate.Experience = therapist.Experience == 0 ? physioToUpdate.Experience : therapist.Experience;
            physioToUpdate.State = string.IsNullOrWhiteSpace(therapist.State) ? physioToUpdate.State : therapist.State;
            physioToUpdate.DOB = string.IsNullOrWhiteSpace(therapist.DOB) ? physioToUpdate.DOB : therapist.DOB;
            physioToUpdate.Age = therapist.Age == 0 ? physioToUpdate.Age : therapist.Age;
            _unitOfWorkPhysio.Repository.Update(physioToUpdate);
            await _unitOfWorkPhysio.Save();
            return new ResponseModel { Status = true, Response = "Update Successful", ReturnObj = physioToUpdate };

        }

        public async Task<ResponseModel> UpdatePatient(Guid patientId, PatientDTO patient)
        {
            var patientToUpdate = await _unitOfWorkPatient.Repository.GetByID(patientId);

            //accept properties to change
            patientToUpdate.FirstName = string.IsNullOrWhiteSpace(patient.FirstName) ? patientToUpdate.FirstName : patient.FirstName;
            patientToUpdate.MiddleName = string.IsNullOrWhiteSpace(patient.MiddleName) ? patientToUpdate.MiddleName : patient.MiddleName;
            patientToUpdate.LastName = string.IsNullOrWhiteSpace(patient.LastName) ? patientToUpdate.LastName : patient.LastName;
            patientToUpdate.DateLastModified = DateTime.UtcNow;
            patientToUpdate.Age = patient.Age == 0 ? patientToUpdate.Age : patient.Age;
            patientToUpdate.DOB = string.IsNullOrWhiteSpace(patient.DOB) ? patientToUpdate.DOB: patient.DOB;
            patientToUpdate.Addressline = string.IsNullOrWhiteSpace(patient.Addressline) ? patientToUpdate.Addressline : patient.Addressline;
            patientToUpdate.PhoneNumber = string.IsNullOrWhiteSpace(patient.PhoneNumber) ? patientToUpdate.PhoneNumber : patient.PhoneNumber;
            patientToUpdate.Country = string.IsNullOrWhiteSpace(patient.Country) ? patientToUpdate.Country : patient.Country;
            patientToUpdate.ProfilePictureUrl = string.IsNullOrWhiteSpace(patient.ProfilePictureUrl) ? patientToUpdate.ProfilePictureUrl : patient.ProfilePictureUrl;
            patientToUpdate.Gender = string.IsNullOrWhiteSpace(patient.Gender) ? patientToUpdate.Gender : patient.Gender;
            patientToUpdate.Email = string.IsNullOrWhiteSpace(patient.Email) ? patientToUpdate.Email : patient.Email;
            patientToUpdate.State = string.IsNullOrWhiteSpace(patient.State) ? patientToUpdate.State : patient.State;


            _unitOfWorkPatient.Repository.Update(patientToUpdate);
            await _unitOfWorkPhysio.Save();
            return new ResponseModel { Status = true, Response = "Update Successful", ReturnObj = patientToUpdate };

        }

        public ResponseModel GetPhysiotherapists(int pageNo)
        {
            try
            {
                var physios = _unitOfWorkPhysio.Repository.GetAllQuery()
                .OrderByDescending(x => x.Ratings)
                .Take(pageNo).ToList();

                return new ResponseModel { Status = true, Response = "Success", ReturnObj = _mapper.Map<PhysiotherapistDTO>(physios) };
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
            return new ResponseModel { Status = true, Response = "Physiotherapist is Verified", ReturnObj = physioToVerify };


        }

        public async Task<ResponseModel> RatePhysiotherapist(Guid therapistId, int value)
        {
            //search physio
            var therapist = await _unitOfWorkPhysio.Repository.GetByID(therapistId);

            //add current to rating data
            therapist.RatingData = string.IsNullOrWhiteSpace(therapist.RatingData) ? $"{value}" : therapist.RatingData + $",{value}";
            //calculate average from rating data
            var ratingData = therapist.RatingData.Split(',');
            var averageRating = Math.Round(Array.ConvertAll(ratingData, delegate (string s) { return Convert.ToInt32(s); }).Average(), 1);
            therapist.Ratings = averageRating;
            try
            {
                _unitOfWorkPhysio.Repository.Update(therapist);
                await _unitOfWorkPhysio.Save();
                return new ResponseModel { Status = true, Response = "Successful" };
            }
            catch (Exception)
            {
                return new ResponseModel { Status = false, Response = "Failed" };
            }
        }

        public async Task<int> ProfileCompletedRate(Guid therapistId)
        {
            // 17 total properties: completed / total * 100
            var therapist = await _unitOfWorkPhysio.Repository.GetByID(therapistId);
            var therapistComp = _mapper.Map<PhysiotherapistCompareCompletedDTO>(therapist);
            var propertiesCompleted = (decimal)therapistComp.GetType().GetProperties().Where(x => x.GetValue(therapistComp) == null).Count();
            var totalProperties = (decimal)17;
            int percentageCompleted = (int)Math.Round((propertiesCompleted / totalProperties) * 100, 0);
            return percentageCompleted;
        }

        public int MyNotifications(Guid UserID)
        {
            return _unitOfWorkNotifications.Repository.GetAllQuery().Count(x => x.OwnerId == UserID && !x.IsRead);
        }

        #endregion

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


        #region PhysiotherapistServices
        // Physiotherapist sessions
        public async Task<ResponseModel> CreateASession(List<PhysioSessionDTO> sessions)
        {
            var physioSessions = new List<PhysioSession>();
            foreach (var item in sessions)
            {
                physioSessions.Add(_mapper.Map<PhysioSession>(item));
            }
            try
            {
                await _unitOfWorkPhysioSessions.Repository.CreateMany(physioSessions);
                await _unitOfWorkPhysioSessions.Save();
                return new ResponseModel { Status = true, Response = "Successful" };
            }
            catch (Exception ex)
            {
                return new ResponseModel { Status = false, Response = ex.Message ?? ex.InnerException.Message };
            }
        }

        public ResponseModel GetATherapistSessions(Guid therapistId)
        {
            var query = _unitOfWorkPhysioSessions.Repository.GetAllQuery().Where(s => s.PhysiotherapistID == therapistId);
            if (query.Any())
            {
                List<PhysioSessionDTO> result = new List<PhysioSessionDTO>();
                foreach (var item in query)
                {
                    result.Add(_mapper.Map<PhysioSessionDTO>(item));
                }
                return new ResponseModel { Status = true, Response = "Successful", ReturnObj = result };
            }
            return new ResponseModel { Status = false, Response = "No session sevice foun for this physiotherapist" };
        }

        #endregion


        #region General
        
        #endregion

    }

}
