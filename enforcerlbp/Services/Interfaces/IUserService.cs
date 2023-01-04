﻿using DTOs.RequestObject;
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
        Task<ResponseModel> GetAPhysioTherapist(Guid Id);
        ResponseModel GetATherapistSessions(Guid therapistId);
        ResponseModel GetPhysiotherapists(int pageNo = 20);
        ResponseModel GetPhysiotherapists(string searchText);
        ResponseModel MyPhysiotherapists(Guid patientId);
        Task<ResponseModel> PatientConnectRequest(ConnectionRequestDTO request);
        Task<ResponseModel> PatientConnectStatus(ConnectionRequestDTO request);
        Task<ResponseModel> PhysiotherapistVerificationFilesUpload(FileDTO document);
        Task<ResponseModel> UpdatePhysiotherapist(Guid physioId, Physiotherapist therapist);
        Task<ResponseModel> Verification(Guid physiotherapistId);
    }
}
