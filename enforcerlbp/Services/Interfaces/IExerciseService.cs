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
        Task<ResponseModel> AddAFeedBackReply(FeedbackReplyDTO reply);
        Task<ResponseModel> AddFeedBackToExercise(FeedbackRequestDTO feedback);
        Task<List<ExercisePrescription>> CheckDefaulters();
        Task<ResponseModel> CompleteExercise(ExerciseCompleteDTO completeExercise);
        Task<ResponseModel> CreateAnExercisePrescription(ExercisePrescriptionDTO request);
        Task<ResponseModel> CreateExercise(ExerciseDTO model);
        Task<ResponseModel> DeclineExercisePrescription(Guid prescriptionId);
        Task<ResponseModel> ExerciseCategory();
        Task<ResponseModel> GetAPrescription(Guid Id);
        Task<ResponseModel> GetExercise(Guid Id);
        ResponseModel GetFeedback(Guid Id);
        ResponseModel GetFeedBacks(Guid patienId);
        ResponseModel GetMyPrescription(Guid ownerId, bool isPatient);
        ResponseModel SuggestedExercise(Guid physiotherapistId);
    }
}
