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
        Task<ResponseModel> CreateExercise(Exercise model);
        Task<ResponseModel> ExerciseCategory();
        Task<ResponseModel> GetExercise(Guid Id);
        ResponseModel SuggestedExercise(Guid physiotherapistId);
    }
}
