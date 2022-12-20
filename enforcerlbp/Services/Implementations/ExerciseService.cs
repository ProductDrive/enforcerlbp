using AutoMapper;
using DataAccess.UnitOfWork;
using DTOs.ResponseObject;
using Entities;
using Entities.Users;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using System;
using System.Collections.Generic;
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
        private readonly IMapper _mapper;

        public ExerciseService(
            IUnitOfWork<Physiotherapist> unitOfWorkPhysio,
            IUnitOfWork<Patient> unitOfWorkPatient,
            IUnitOfWork<Exercise> unitOfWorkExercise,
            IUnitOfWork<ExercisePrescription> unitOfWorkExercisePrescription,
            IMapper mapper
            )
        {
            _unitOfWorkPhysio = unitOfWorkPhysio;
            _unitOfWorkPatient = unitOfWorkPatient;
            _unitOfWorkExercise = unitOfWorkExercise;
            _unitOfWorkExercisePrescription = unitOfWorkExercisePrescription;
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
    }
}

