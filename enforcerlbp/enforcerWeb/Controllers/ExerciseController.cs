﻿using DTOs.RequestObject;
using DTOs.ResponseObject;
using enforcerWeb.Helper;
using Entities;
using Infrastructures.EmailServices;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace enforcerWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExerciseController : ControllerBase
    {
        private readonly IExerciseService _exerciseService;
        private readonly IMediator _mediatR;

        public ExerciseController(IExerciseService exerciseService, IMediator mediatR)
        {
            _exerciseService = exerciseService;
            _mediatR = mediatR;
        }

        [HttpPost("completed")]
        public async Task<IActionResult> ExerciseCompletion(ExerciseCompleteDTO exerComplete)
        {
            var result = await _exerciseService.CompleteExercise(exerComplete);
            var execPrescribed = JsonConvert.DeserializeObject<ExercisePrescription>(JsonConvert.SerializeObject(result));

            // Send notification to physiotherapist
            var ownerIds = new List<Guid>() { execPrescribed.PhysiotherapistId};
            string gender = execPrescribed.Patient.Gender.ToLower() == "male" ? "his" : "her";
            string message = $"{execPrescribed.Patient.FirstName} completed {gender} exercise";
            await _mediatR.Send(NotificationHelper.GetNotificationModelManyOwnersOneMessage(ownerIds, message));
            //send email
            await _mediatR.Send(new EmailSenderCommand
            {
                Contacts = new List<ContactsModel>
                     {
                         new ContactsModel
                         {
                              Email = execPrescribed.Physiotherapist.Email,
                              Name = execPrescribed.Physiotherapist.FirstName
                         }
                     },
                EmailDisplayName = "Health Enforcer",
                Subject = $"Exercise Completion",
                Message = message
            });
            return Ok(result);
        }

        [HttpGet("myexercises")]
        public IActionResult GetMyExercises(Guid ownerId, bool isPatient) => Ok(_exerciseService.GetMyPrescription (ownerId, isPatient));

        [HttpGet("prescription")]
        public async Task<IActionResult> GetAPrescription(Guid id) => Ok(await _exerciseService.GetAPrescription(id));

        [HttpPost("prescription")]
        public async Task<IActionResult> AddExercisePrescription(ExercisePrescriptionDTO request)
        {
            var result = await _exerciseService.CreateAnExercisePrescription(request);
            string jsonformOfresult = JsonConvert.SerializeObject(result.ReturnObj);
            ExercisePrescription eP = JsonConvert.DeserializeObject<ExercisePrescription>(jsonformOfresult);

            //send notification patient
            var ownerIds = new List<Guid>() { eP.PatientId };
            string message = $"An exercise has be prescribed for you. Kindly check your list of exercise prescriptions";
            await _mediatR.Send(NotificationHelper.GetNotificationModelManyOwnersOneMessage(ownerIds, message));
            //send email
            await _mediatR.Send(new EmailSenderCommand
            {
                Contacts = new List<ContactsModel>
                     {
                         new ContactsModel
                         {
                              Email = eP.Patient.Email,
                              Name = eP.Patient.FirstName
                         }
                     },
                EmailDisplayName = "Health Enforcer",
                Subject = $"Exercise Prescription",
                Message = message
            });
            return Ok(result);
        }
    }
}

