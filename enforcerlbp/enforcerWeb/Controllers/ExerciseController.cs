using DTOs.RequestObject;
using DTOs.ResponseObject;
using enforcerWeb.Helper;
using Entities;
using Infrastructures.EmailServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
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

        [HttpPost]
        //[Authorize(Policy = "Admin")]
        public async Task<ResponseModel> AddExercise(ExerciseDTO model) => await _exerciseService.CreateExercise(model);

        [HttpPost("uploadmany")]
        [AllowAnonymous]
        //[Authorize(Policy = "Admin")]
        public async Task<IActionResult> AddManyExercise(List<UploadExerciseDTO> models)
        {
            foreach (var model in models)
            {

                await _exerciseService.CreateExercise(
                    new ExerciseDTO
                    {
                        Category = model.Category,
                        DateCreated = DateTime.Now,
                        Description = model.Description,
                        FileUrl = model.VideoURL,
                        ID = Guid.NewGuid(),
                        IsSuggested = false,
                        Name = model.Name
                    });
            }
            return Ok();
        }

        [HttpGet]
        [Authorize(Policy = "Users")]
        public async Task<ResponseModel> GetOneExercise(Guid Id) => await _exerciseService.GetExercise(Id);

        [HttpGet("categorized")]
        [Authorize(Policy = "Users")]
        public async Task<ResponseModel> GetExerciseByCategories() => await _exerciseService.ExerciseCategory();

        [HttpPost("completed")]
        [Authorize(Policy = "Patient")]
        public async Task<IActionResult> ExerciseCompletion([FromForm]ExerciseCompleteDTO exerComplete)
        {

            var result = await _exerciseService.CompleteExercise(exerComplete);
            var execPrescribed = JsonConvert.DeserializeObject<ExercisePrescription>(JsonConvert.SerializeObject(result.ReturnObj));

            // Send notification to physiotherapist
            var ownerIds = new List<Guid>() { execPrescribed.PhysiotherapistId};
            string gender = execPrescribed.Patient.Gender?.ToLower() == "male" ? "his" : "her";
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

        [HttpPost("completedlive")]
        [Authorize(Policy = "Users")]
        public async Task<IActionResult> LiveExerciseCompletion(ExerciseCompleteDTO exerComplete)
        {

            var result = await _exerciseService.CompleteExercise(exerComplete);
            var execPrescribed = JsonConvert.DeserializeObject<ExercisePrescription>(JsonConvert.SerializeObject(result.ReturnObj));

            // Send notification to physiotherapist
            var ownerIds = new List<Guid>() { execPrescribed.PhysiotherapistId };
            string gender = !string.IsNullOrWhiteSpace(execPrescribed.Patient.Gender)? execPrescribed.Patient.Gender.ToLower() == "male" ? "his" : "her":"their";
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
            result.ReturnObj = null;
            return Ok(result);
        }


        [HttpGet("myexercises")]
        [Authorize(Policy = "AppUser")]
        public IActionResult GetMyExercises(Guid ownerId, bool isPatient) => Ok(_exerciseService.GetMyPrescription (ownerId, isPatient));

        [HttpGet("prescription")]
        [Authorize(Policy = "Users")]
        public async Task<IActionResult> GetAPrescription(Guid id) => Ok(await _exerciseService.GetAPrescription(id));

        [HttpPost("prescription")]
        [Authorize(Policy = "Therapist")]
        public async Task<IActionResult> AddExercisePrescription(ExercisePrescriptionDTO request)
        {
            var result = await _exerciseService.CreateAnExercisePrescription(request);
            string jsonformOfresult = JsonConvert.SerializeObject(result.ReturnObj);
            ExercisePrescription eP = JsonConvert.DeserializeObject<ExercisePrescription>(jsonformOfresult);

            //send notification patient
            var ownerIds = new List<Guid>() { eP.PatientId };
            string message = $"An exercise has been prescribed for you. Kindly check your list of exercise prescriptions";
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

        //FEEDBACK ENDPOINTS
        [HttpPost("addfeedback")]
        [Authorize(Policy = "Patient")]
        public async Task<ResponseModel> ExerciseFeedback(FeedbackRequestDTO model) => await _exerciseService.AddFeedBackToExercise(model);

        [HttpPost("feedbackreply")]
        [Authorize(Policy = "AppUser")]
        public async Task<ResponseModel> AddAReply(FeedbackReplyDTO model)
        {
            var response = await _exerciseService.AddAFeedBackReply(model);
            var ownerId = new List<Guid>() { model.OwnerId };
            string message = $"There is a reply to your feedback on {model.ExerciseName}.";
            await _mediatR.Send(NotificationHelper.GetNotificationModelManyOwnersOneMessage(ownerId, message));
            return response;
        }

        [HttpGet("feedbacklist")]
        [Authorize(Policy = "AppUser")]
        public ResponseModel GetFeedbacks(Guid patientId) => _exerciseService.GetFeedBacks(patientId);

        [HttpGet("feedback")]
        [Authorize(Policy = "AppUser")]
        public ResponseModel GetOneFeedback(Guid Id) => _exerciseService.GetFeedback(Id);
        //send notification for feedback replies
    }
}

