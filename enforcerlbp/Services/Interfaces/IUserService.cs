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
    }
}
