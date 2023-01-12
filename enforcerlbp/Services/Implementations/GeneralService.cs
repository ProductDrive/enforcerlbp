using DTOs.RequestObject;
using DTOs.ResponseObject;
using Newtonsoft.Json;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class GeneralService: IGeneralService
    {
        private readonly IUserService _userService;
        private readonly IExerciseService _exerciseService;

        public GeneralService(IUserService userService, IExerciseService exerciseService)
        {
            _userService = userService;
            _exerciseService = exerciseService;
        }

        #region Dashboard

        public async Task<ResponseModel> GetTherapistDashboardData(Guid therapistID)
        {
            //The therapist
            var therapistRes = await _userService.GetAPhysioTherapist(therapistID);
            string jsonformOfresult = JsonConvert.SerializeObject(therapistRes.ReturnObj);
            var therapist = JsonConvert.DeserializeObject<PhysiotherapistDTO>(jsonformOfresult);

            //patients
            var result = _userService.GetMyPatients(therapistID);
            string jsonresult = JsonConvert.SerializeObject(result.ReturnObj);
            var patients = JsonConvert.DeserializeObject<List<PatientDTO>>(jsonformOfresult);

            //No of patients
            var numberOfPatients = patients.Count;

            //percentage profile completed
            var percentageCompleted = await _userService.ProfileCompletedRate(therapistID);

            //Wallet Balance
            var balance = new { Amount = 0, Status = "Inactive" };

            //Notification
            var notifications = _userService.MyNotifications(therapistID);

            return new ResponseModel 
                { 
                    Status = true, 
                    Response = "Successful", 
                    ReturnObj = new 
                    {
                        Therapist = therapist,
                        Patient = patients,
                        PatientCount = numberOfPatients,
                        ProfileCompleted = percentageCompleted,
                        WalletBalance = balance,
                        Notification = notifications
                    } };

        }

        #endregion

    }
}
