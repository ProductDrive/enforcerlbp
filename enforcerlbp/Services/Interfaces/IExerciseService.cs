using DTOs.RequestObject;
using DTOs.ResponseObject;
using Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IExerciseService
    {
        Task<List<ExercisePrescription>> CheckDefaulters();
        Task<ResponseModel> CompleteExercise(ExerciseCompleteDTO completeExercise);
        Task<ResponseModel> CreateAnExercisePrescription(ExercisePrescriptionDTO request);
        Task<ResponseModel> CreateExercise(Exercise model);
        Task<ResponseModel> ExerciseCategory();
        Task<ResponseModel> GetAPrescription(Guid Id);
        Task<ResponseModel> GetExercise(Guid Id);
        ResponseModel GetMyPrescription(Guid ownerId, bool isPatient);
        ResponseModel SuggestedExercise(Guid physiotherapistId);
    }
}
