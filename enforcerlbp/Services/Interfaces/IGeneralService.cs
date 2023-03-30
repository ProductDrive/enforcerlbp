using DTOs.ResponseObject;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IGeneralService
    {
        Task<ResponseModel> GetPatientDashboardData(Guid patientID);
        Task<ResponseModel> GetTherapistDashboardData(Guid therapistID);
    }
}
