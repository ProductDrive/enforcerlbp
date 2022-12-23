using DTOs.RequestObject;
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
    }
}

