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
        Task<ResponseModel> CreatePatient(PatientDTO model);
        Task<ResponseModel> CreatePhysiotherapist(PhysiotherapistDTO model);
        Task<ResponseModel> GetAPatient(Guid Id);
        Task<ResponseModel> GetAPhysioTherapist(Guid Id);
        ResponseModel GetPhysiotherapists(int pageNo);
        ResponseModel GetPhysiotherapists(string searchText);
        ResponseModel MyPhysiotherapists(Guid patientId);
        Task<ResponseModel> PatientConnectRequest(ConnectionRequestDTO request);
        Task<ResponseModel> PatientConnectStatus(ConnectionRequestDTO request);
        Task<ResponseModel> PhysiotherapistVerificationFilesUpload(FileDTO document);
        Task<ResponseModel> RatePhysiotherapist(Guid therapistId, int value);
        Task<ResponseModel> UpdatePhysiotherapist(Guid physioId, Physiotherapist therapist);
        Task<ResponseModel> Verification(Guid physiotherapistId);
    }
}
