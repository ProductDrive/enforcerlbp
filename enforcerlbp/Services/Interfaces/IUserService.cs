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
        Task<ResponseModel> DeleteNotification(Guid notificationId);
        ResponseModel GetAllConnections(Guid patientId);
        Task<ResponseModel> GetAPatient(Guid Id);
        Task<ResponseModel> GetAPhysioTherapist(Guid Id);
        ResponseModel GetATherapistSessions(Guid therapistId);
        ResponseModel GetMyPatients(Guid therapistId);
        ResponseModel GetMyPhysiotherapist(Guid patientId);
        ResponseModel GetNotifications(Guid ownerId);
        ResponseModel GetPendingConnections(Guid patientId);
        ResponseModel GetPendingNotifications(Guid ownerId);
        ResponseModel GetPhysiotherapists(int pageNo);
        ResponseModel GetPhysiotherapists(string searchText);
        ResponseModel GetSeenNotifications(Guid ownerId);
        int MyNotifications(Guid UserID);
        ResponseModel MyPhysiotherapists(Guid patientId);
        Task<ResponseModel> PatientConnectRequest(ConnectionsDTO request);
        Task<ResponseModel> PatientConnectStatus(ConnectionsDTO request);
        Task<ResponseModel> PhysiotherapistVerificationFilesUpload(FileDTO document);
        Task<int> ProfileCompletedRate(Guid therapistId);
        Task<ResponseModel> RatePhysiotherapist(Guid therapistId, int value);
        Task<ResponseModel> ReadNotification(Guid notificationId);
        Task<ResponseModel> UpdatePatient(Guid patientId, PatientDTO patient);
        Task<ResponseModel> UpdatePhysiotherapist(Guid physioId, PhysiotherapistDTO therapist);
        Task<ResponseModel> Verification(Guid physiotherapistId);
    }
}
