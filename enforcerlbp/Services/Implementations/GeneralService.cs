using DTOs.RequestObject;
using DTOs.ResponseObject;
using Entities;
using Newtonsoft.Json;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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
            try
            {
                //The therapist
                var therapistRes = await _userService.GetAPhysioTherapist(therapistID);
                string jsonformOfresult = JsonConvert.SerializeObject(therapistRes.ReturnObj);
                var therapist = JsonConvert.DeserializeObject<PhysiotherapistDTO>(jsonformOfresult);

                //patients
                var result = _userService.GetMyPatients(therapistID);
                List<PatientDTO> patients = new List<PatientDTO>();
                if (result.ReturnObj != null)
                {
                    string jsonresult = JsonConvert.SerializeObject(result.ReturnObj);
                    patients = JsonConvert.DeserializeObject<List<PatientDTO>>(jsonresult);
                }

                //No of patients
                var numberOfPatients = patients.Any() ? patients.Count : 0;

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
                    }
                };
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        public async Task<ResponseModel> GetPatientDashboardData(Guid patientID)
        {
            //notification
            var notifications = _userService.MyNotifications(patientID);
            //The patient
            var patientRes = await _userService.GetAPatient(patientID);
            string jsonformOfresult = JsonConvert.SerializeObject(patientRes.ReturnObj);
            var patient = JsonConvert.DeserializeObject<PatientDTO>(jsonformOfresult);

            //physiotherapist
            var result = _userService.GetMyPhysiotherapist(patientID);
            List<PhysiotherapistDTO> physioDto = new List<PhysiotherapistDTO>();
            if (result.ReturnObj != null)
            {
                string jsonresult = JsonConvert.SerializeObject(result.ReturnObj);
                physioDto = JsonConvert.DeserializeObject<List<PhysiotherapistDTO>>(jsonresult);
            }

            //health records
            var healthresult = await _exerciseService.GetAPrescription(patientID);
            var healthRec = new HealthRecordDTO();
            List<ExercisePrescription> patientExerciseRec = new List<ExercisePrescription>();
            if (result.ReturnObj != null)
            {
                string jsonExerresult = JsonConvert.SerializeObject(healthresult.ReturnObj);
                patientExerciseRec = JsonConvert.DeserializeObject<List<ExercisePrescription>>(jsonExerresult);
                healthRec.CompletedExercises = patientExerciseRec?.Count(x => x.IsCompleted) ?? 0;
                healthRec.OngoingExercises = patientExerciseRec?.Count(x => !x.IsCompleted) ?? 0;
                healthRec.TotalPrescribedExercise = patientExerciseRec?.Count() ?? 0;
            }

            return new ResponseModel
            {
                Status = true,
                Response = "Successful",
                ReturnObj = new
                {
                    Patient = patient,
                    Physiotherapist = physioDto,
                    Notification = notifications,
                    PhysiotherapistCount = physioDto.Count,
                    HealthRecord = healthRec
                }
            };

        
    }
        #endregion

        

    }
}
