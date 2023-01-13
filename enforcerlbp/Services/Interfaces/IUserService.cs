using DTOs.RequestObject;
using DTOs.ResponseObject;
using Entities.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IUserService
    {
        Task<ResponseModel> CreateASession(List<PhysioSessionDTO> sessions);
        Task<ResponseModel> CreatePatient(PatientDTO model);
        Task<ResponseModel> CreatePhysiotherapist(PhysiotherapistDTO model);
        Task<ResponseModel> GetAPatient(Guid Id);
        Task<ResponseModel> GetAPhysioTherapist(Guid Id);
        ResponseModel GetATherapistSessions(Guid therapistId);
        ResponseModel GetMyPatients(Guid therapistId);
        ResponseModel GetMyPhysiotherapist(Guid patientId);
        ResponseModel GetPhysiotherapists(int pageNo);
        ResponseModel GetPhysiotherapists(string searchText);
        int MyNotifications(Guid UserID);
        ResponseModel MyPhysiotherapists(Guid patientId);
        Task<ResponseModel> PatientConnectRequest(ConnectionRequestDTO request);
        Task<ResponseModel> PatientConnectStatus(ConnectionRequestDTO request);
        Task<ResponseModel> PhysiotherapistVerificationFilesUpload(FileDTO document);
        Task<int> ProfileCompletedRate(Guid therapistId);
        Task<ResponseModel> RatePhysiotherapist(Guid therapistId, int value);
        Task<ResponseModel> UpdatePatient(Guid patientId, PatientDTO patient);
        Task<ResponseModel> UpdatePhysiotherapist(Guid physioId, PhysiotherapistDTO therapist);
        Task<ResponseModel> Verification(Guid physiotherapistId);
    }
}
