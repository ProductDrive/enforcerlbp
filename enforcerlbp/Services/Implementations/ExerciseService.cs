using AutoMapper;
using DataAccess.UnitOfWork;
using DTOs.RequestObject;
using DTOs.ResponseObject;
using Entities;
using Entities.Users;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class ExerciseService:IExerciseService
    {
        private readonly IUnitOfWork<Physiotherapist> _unitOfWorkPhysio;
        private readonly IUnitOfWork<Patient> _unitOfWorkPatient;
        private readonly IUnitOfWork<Exercise> _unitOfWorkExercise;
        private readonly IUnitOfWork<ExercisePrescription> _unitOfWorkExercisePrescription;
        private readonly IUnitOfWork<Feedback> _unitOfWorkFeedback;
        private readonly IUnitOfWork<FeedbackReply> _unitOfWorkReplies;
        private readonly IFirebase _firebaseService;
        private readonly IMapper _mapper;

        public ExerciseService(
            IUnitOfWork<Physiotherapist> unitOfWorkPhysio,
            IUnitOfWork<Patient> unitOfWorkPatient,
            IUnitOfWork<Exercise> unitOfWorkExercise,
            IUnitOfWork<ExercisePrescription> unitOfWorkExercisePrescription,
            IUnitOfWork<Feedback> unitOfWorkFeedback,
            IUnitOfWork<FeedbackReply> unitOfWorkReplies,
            IFirebase firebaseService,
            IMapper mapper
            )
        {
            _unitOfWorkPhysio = unitOfWorkPhysio;
            _unitOfWorkPatient = unitOfWorkPatient;
            _unitOfWorkExercise = unitOfWorkExercise;
            _unitOfWorkExercisePrescription = unitOfWorkExercisePrescription;
            _unitOfWorkFeedback = unitOfWorkFeedback;
            _unitOfWorkReplies = unitOfWorkReplies;
            _firebaseService = firebaseService;
            _mapper = mapper;
        }
        #region Exercise Methods
        public async Task<ResponseModel> GetExercise(Guid Id)
        {
            var exercise = await _unitOfWorkExercise.Repository.GetByID(Id);
            return new ResponseModel { Status = true, Response= "Successful", ReturnObj = exercise };
        }
        public async Task<ResponseModel> CreateExercise(ExerciseDTO model)
        {
            var exe = _mapper.Map<Exercise>(model);
            await _unitOfWorkExercise.Repository.Create(exe);
            await _unitOfWorkExercise.Save();
            return new ResponseModel { Status = true, Response = "Exercise created successfully" };
        }

        public async Task<ResponseModel> ExerciseCategory()
        {
            var exercises = await _unitOfWorkExercise.Repository.GetAll();
            var exerciseCategory = exercises.GroupBy(s => s.Category).Select(x=> new {Category = x.Key, Exercises = x.ToList()});
            return new ResponseModel { Status = true, Response="Successful", ReturnObj = exerciseCategory};

        }

        #endregion

        #region Exercise Prescription

        public ResponseModel SuggestedExercise(Guid physiotherapistId)
        {
            var latestPrescription = _unitOfWorkExercisePrescription.Repository.GetAllQuery()
                 .Include(s => s.Exercise)
                 .Where(s => s.PhysiotherapistId == physiotherapistId)
                 .OrderByDescending(s => s.DateCreated).Take(4);
            if (latestPrescription != null && latestPrescription.Any())
            {
                var suggestedExercise = latestPrescription.ToList()
                    .Select(s => s.Exercise).ToList();
                return new ResponseModel { Status = true, Response = "Successful", ReturnObj = suggestedExercise };
            }
            return new ResponseModel { Status = false, Response = "failed" };
        }

        public ResponseModel GetMyPrescription(Guid ownerId, bool isPatient)
        {
            var response = new ResponseModel();
            try
            {
                if (isPatient)
                {
                    response.ReturnObj = _unitOfWorkExercisePrescription.Repository.GetAllQuery()
                    .Include(x=>x.Physiotherapist)
                    .Where(x => x.PatientId == ownerId);
                }
                else
                {
                    response.ReturnObj = _unitOfWorkExercisePrescription.Repository.GetAllQuery()
                    .Include(x=>x.Patient)
                    .Where(x => x.PhysiotherapistId == ownerId);
                }
                response.Status = true;
                response.Response = "Successful";
                return response;
            }
            catch (Exception)
            {
                return new ResponseModel { Status = false, Response = "Something went wrong while get exercise prescription" };
            }
            

        }

        public async Task<ResponseModel> GetAPrescription(Guid Id)
        {
            return new ResponseModel { Status = true, Response="Successful", ReturnObj = await _unitOfWorkExercisePrescription.Repository.GetByID(Id) };
        }

        public async Task<ResponseModel> CreateAnExercisePrescription(ExercisePrescriptionDTO request)
        {
            if (request.PatientId == Guid.Empty
                && request.PhysiotherapistId == Guid.Empty
                && request.IsCompleted
                && string.IsNullOrWhiteSpace(request.ExerciseName)
                && request.ExerciseId == Guid.Empty
                )
            {
                return new ResponseModel { Status = false, Response = "parameter error", ReturnObj = request };
            }

            if (request.StartDate == default) request.StartDate = DateTime.Now;
            if (request.EndDate == default) request.EndDate = DateTime.Now.AddDays(5);

            var prescription = _mapper.Map<ExercisePrescription>(request);
            prescription.DateCreated = DateTime.Now;

            try
            {
                await _unitOfWorkExercisePrescription.Repository.Create(prescription);
                await _unitOfWorkExercisePrescription.Save();
                //TODO: send notification and email to patient
                prescription.Patient = new Patient();
                prescription.Patient = await _unitOfWorkPatient.Repository.GetByID(request.PatientId);
                return new ResponseModel { Status = true, Response = "Save successfully", ReturnObj = prescription };
            }
            catch (Exception ex)
            {
                return new ResponseModel { Status = false, Response = $"{ex.Message??ex.InnerException.Message}", ReturnObj = request };
            }

            
        }


        //2 private method. 1st will exercisepreescription by id
        //2nd get exerciseprescription by feedback id
        //public method that takes the two as parameter exercisePrescription and feedback id as optionalparameter
        #endregion

        #region Exercise Monitoring
        public async Task<ResponseModel> CompleteExercise(ExerciseCompleteDTO completeExercise)
        {
            var execPrescribed = _unitOfWorkExercisePrescription.Repository.GetAllQuery()
                .Include(x=>x.Physiotherapist)
                .Include(x=>x.Patient)
                .Include(x=>x.Exercise)
                .FirstOrDefault(x=>x.ID == completeExercise.ExercisePrescriptionId);

           // var result = new ResponseModel();
           if(completeExercise.Video != null &&  completeExercise.Video.Length < 1 && completeExercise.IsLiveMonitored == false) return new ResponseModel { Status = false, Response = "Video is empty" };

          
            if (execPrescribed == null) return new ResponseModel { Status = false, Response = "Invalid Execise Id or No assigned exercise with the Id" };

            if (completeExercise.IsLiveMonitored)
            {
                execPrescribed.IsCompleted = true;
                execPrescribed.DateModified = DateTime.Now;
                _unitOfWorkExercisePrescription.Repository.Update(execPrescribed);
                await _unitOfWorkExercisePrescription.Save();

                return new ResponseModel { Status = true, Response = "Successful End of session", ReturnObj = execPrescribed };

            }

            if (completeExercise.Video != null &&  completeExercise.Video.Length > 1)
            {
                var response = await UploadFileForExercise(completeExercise, execPrescribed.PatientId.ToString());
                if (!string.IsNullOrWhiteSpace(response.Response))
                {
                    execPrescribed.SubmittedVideoUrl = response.Response;
                    execPrescribed.IsCompleted = true;
                    execPrescribed.DateModified = DateTime.Now;
                    _unitOfWorkExercisePrescription.Repository.Update(execPrescribed);
                    await _unitOfWorkExercisePrescription.Save();

                    return new ResponseModel { Status = true, Response = "Exercise response saved successfully. Physiotherapist will have a look at it and revert.", ReturnObj = execPrescribed };

                }
            }
            
            return new ResponseModel { Status = false, Response = "Exercise video could not be saved at this time. Kindly try again or contact Enforcer help centre", ReturnObj = execPrescribed };
         
        }

        public async Task<ResponseModel> DeclineExercisePrescription(Guid prescriptionId)
        {
            try
            {
                var execPrescribed = await _unitOfWorkExercisePrescription.Repository.GetByID(prescriptionId);
                if (execPrescribed == null)
                {
                    return new ResponseModel { Status = false, Response = "Excercise not found" };
                }
                execPrescribed.IsCompleted = false;
                _unitOfWorkExercisePrescription.Repository.Update(execPrescribed);
                await _unitOfWorkExercisePrescription.Save();
                return new ResponseModel { Status = true, Response = "Declined Successfully" };
            }
            catch (Exception ex)
            {
                return new ResponseModel { Status = false, Response = "An error occured while trying to decline. Contact Admin", Errors = new List<string> {ex.InnerException.Message ?? ex.Message } };
            }
        }

        private async Task<ResponseModel> UploadFileForExercise(ExerciseCompleteDTO completeExercise, string patientId)
        {
            string[] allowedExtensions = { ".mp4", ".wav", ".3pg", ".mov", ".webm", "flv", ".wmv", ".avi" };
            string videoExtension = Path.GetExtension(completeExercise.Video.FileName);

            //TODO: refactor the if statement
            if (!allowedExtensions.Contains(videoExtension))
            {
                return new ResponseModel { Status = false, Response = "File select is not a supported video format. Supported formats: .mp4, .wav, .3pg, .mov, .webm, .flv, .wmv, .avi" };
            }
            // rename the file so that it can be unique file per user
           
            var returnedVedioUrl = await _firebaseService.FirebaseFileUpload(completeExercise.Video, $"CompletedExercises|{patientId}");
            return new ResponseModel {Response = returnedVedioUrl };
        }

        public async Task<List<ExercisePrescription>> CheckDefaulters()
        {
            var today = DateTime.Today;
            var allPrescription = _unitOfWorkExercisePrescription.Repository.GetAllQuery()
                .Where(x => !x.IsCompleted && x.EndDate.Date > today)
                .Select(x => new ExercisePrescription
                {
                    ID = x.ID,
                    EndDate = x.EndDate,
                    PatientId = x.PatientId,
                    Patient = new Patient { ID = x.Patient.ID, FirstName = x.Patient.FirstName, LastName = x.Patient.LastName },
                    Physiotherapist = new Physiotherapist { ID = x.Physiotherapist.ID, FirstName = x.Physiotherapist.FirstName },
                    ExerciseId = x.ExerciseId,
                    Exercise = new Exercise { ID = x.Exercise.ID, Name = x.Exercise.Name }
                });

            if (!allPrescription.Any()) return null;

            return await allPrescription.ToListAsync();
        }



        #endregion


        #region Exercise Feedback and Replies
        //create feedback
        public async Task<ResponseModel> AddFeedBackToExercise(FeedbackRequestDTO feedback)
        {
            try
            {
                var fb = _mapper.Map<Feedback>(feedback);
                fb.DateCreated = DateTime.Now;
                await _unitOfWorkFeedback.Repository.Create(fb);
                await _unitOfWorkFeedback.Save();
                return new ResponseModel { Status = true, Response = "Successful" };
            }
            catch (Exception ex)
            {
                return new ResponseModel { Status = false, Response = ex.Message??ex.InnerException.Message };
            }
        }
        
        //add reply to feedback - send notification
        public async Task<ResponseModel> AddAFeedBackReply(FeedbackReplyDTO reply) 
        {
            try
            {
                var fbr = _mapper.Map<FeedbackReply>(reply);
                var query = _unitOfWorkReplies.Repository.GetAllQuery().Where(x => x.FeedbackId == reply.FeedbackId);
                fbr.OrderId = query.Count() > 0 ? query.Count() + 1 : 1;
                await _unitOfWorkReplies.Repository.Create(fbr);
                await _unitOfWorkReplies.Save();
                return new ResponseModel { Status = true, Response = "Successful", ReturnObj = reply};
            }
            catch (Exception ex)
            {
                return new ResponseModel { Status = false, Response = ex.Message ?? ex.InnerException.Message };
            }
        }

        // get a list of feedback
        public ResponseModel GetFeedBacks(Guid patienId)
        {
           var feedbackss = _unitOfWorkFeedback.Repository.GetAllQuery().Where(x => x.PatientId == patienId);
            if (feedbackss.Any())
            {
                var feedbackResult = new List<FeedbackResponseDTO>();
                foreach (var item in feedbackss)
                {
                    feedbackResult.Add(_mapper.Map<FeedbackResponseDTO>(item));
                }
                return new ResponseModel { Status = true, Response = "Successful", ReturnObj = feedbackResult };
            }
            else
            {
                return new ResponseModel { Status = false, Response = "This patient has not completed any feedback yet"};
            }

        }
        // get feedback - attach its replies in hirachical order
        public ResponseModel GetFeedback(Guid Id)
        {
           var query = _unitOfWorkFeedback.Repository.GetAllQuery().Include(r=>r.FeedbackReplies).FirstOrDefault(x => x.ID == Id);
            if (query != null)
            {
                if (query.FeedbackReplies != null)
                {
                    query.FeedbackReplies.OrderByDescending(r => r.OrderId);
                    var res = _mapper.Map<FeedbackResponseDTO>(query);
                    return new ResponseModel { Status = true, Response = "Successful", ReturnObj = res };
                }
                return new ResponseModel { Status = true, Response = "Successful", ReturnObj = query };
            }
            return new ResponseModel { Status = false, Response = "Failed"};
        }
        #endregion
    }
}    

