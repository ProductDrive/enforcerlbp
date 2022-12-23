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
        private readonly IFirebase _firebaseService;
        private readonly IMapper _mapper;

        public ExerciseService(
            IUnitOfWork<Physiotherapist> unitOfWorkPhysio,
            IUnitOfWork<Patient> unitOfWorkPatient,
            IUnitOfWork<Exercise> unitOfWorkExercise,
            IUnitOfWork<ExercisePrescription> unitOfWorkExercisePrescription,
            IFirebase firebaseService,
            IMapper mapper
            )
        {
            _unitOfWorkPhysio = unitOfWorkPhysio;
            _unitOfWorkPatient = unitOfWorkPatient;
            _unitOfWorkExercise = unitOfWorkExercise;
            _unitOfWorkExercisePrescription = unitOfWorkExercisePrescription;
            _firebaseService = firebaseService;
            _mapper = mapper;
        }
        #region Exercise Methods
        public async Task<ResponseModel> GetExercise(Guid Id)
        {
            var exercise = await _unitOfWorkExercise.Repository.GetByID(Id);
            return new ResponseModel { Status = true, ReturnObj = exercise };
        }
        public async Task<ResponseModel> CreateExercise(Exercise model)
        {
            await _unitOfWorkExercise.Repository.Create(model);
            await _unitOfWorkExercise.Save();
            return new ResponseModel { Status = true, Response = "Exercise created successfully" };
        }

        public async Task<ResponseModel> ExerciseCategory()
        {
            var exercises = await _unitOfWorkExercise.Repository.GetAll();
            var exerciseCategory = exercises.GroupBy(s => s.Category);
            return new ResponseModel { Status = true, Response="Successful", ReturnObj = exerciseCategory };

        }
        public  ResponseModel SuggestedExercise( Guid physiotherapistId)
        {
           var latestPrescription = _unitOfWorkExercisePrescription.Repository.GetAllQuery()
                .Include(s => s.Exercise)
                .Where(s => s.PhysiotherapistId == physiotherapistId)
                .OrderByDescending(s => s.DateCreated).Take(4);
            if(latestPrescription != null && latestPrescription.Any())
            {
                var suggestedExercise = latestPrescription.ToList()
                    .Select(s => s.Exercise).ToList();
                return new ResponseModel { Status = true, Response = "Successful", ReturnObj = suggestedExercise };
            }
            return new ResponseModel { Status = false, Response = "failed" };
   

        }
        #endregion

        #region ExercisePrescription
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

            if (execPrescribed == null) return new ResponseModel { Status = false, Response = "Invalid Execise Id or No assigned exercise with the Id" };

            if (completeExercise.Video.FileName.Length < 1) return new ResponseModel { Status = false, Response = "Video is empty" };
            string[] allowedExtensions = { ".mp4", ".wav", ".3pg", ".mov", ".webm", "flv", ".wmv", ".avi" };
            string videoExtension = Path.GetExtension(completeExercise.Video.FileName);

            //TODO: refactor the if statement
            if (!allowedExtensions.Contains(videoExtension))
            {
                return new ResponseModel { Status = false, Response = "File select is not a supported video format. Supported formats: .mp4, .wav, .3pg, .mov, .webm, .flv, .wmv, .avi" };
            }
            var returnedVedioUrl = await _firebaseService.FirebaseFileUpload(completeExercise.Video, "CompletedExercises");
            execPrescribed.SubmittedVideoUrl = returnedVedioUrl;
            execPrescribed.IsCompleted = true;
            execPrescribed.DateModified = DateTime.Now;

            _unitOfWorkExercisePrescription.Repository.Update(execPrescribed);
            await _unitOfWorkExercisePrescription.Save();

            

            return new ResponseModel { Status = true, Response = "Exercise response saved successfully. Physiotherapist will have a look at it and revert.", ReturnObj = execPrescribed };
            //TODO: send notification in controller

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

    }
}    

