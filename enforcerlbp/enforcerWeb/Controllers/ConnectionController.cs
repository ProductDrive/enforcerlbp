using AutoMapper;
using DTOs.RequestObject;
using DTOs.ResponseObject;
using enforcerWeb.Helper;
using Entities.Users;
using Infrastructures.EmailServices;
using Infrastructures.NotificationService;
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
    [ApiController]
    public class ConnectionController:ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediatR;
        private readonly IUserService _userService;

        public ConnectionController(IMapper mapper, IMediator mediatR, IUserService userService)
        {
            _mapper = mapper;
            _mediatR = mediatR;
            _userService = userService;
        }

        [Authorize(Policy = "Patient")]
        [HttpPost("sendconnection")]
        public async Task<IActionResult> ConnectToATherapist(ConnectionRequestDTO request)
        {
            var result = await _userService.PatientConnectRequest(request);
            if (result.Status)
            {
                //send notification
                var connectionMessage = NotificationHelper.GetNotificationMessage(request).Split('|');
                await _mediatR.Send(NotificationHelper.GetNotificationModelManyOwnersManyMessages(new List<NotificationRequester>()
                {
                    new NotificationRequester{ Message = connectionMessage[0], OwnerID = request.PatientID},
                    new NotificationRequester{ Message = connectionMessage[1], OwnerID = request.PhysiotherapistID}
                } ));
                var query = await _userService.GetAPhysioTherapist(request.PhysiotherapistID);
                var physio = JsonConvert.DeserializeObject<PhysioTherapistConnectDTO>(JsonConvert.SerializeObject(query.ReturnObj));
                //send email
                await _mediatR.Send(new EmailSenderCommand
                {
                    Contacts = new List<ContactsModel>
                     {
                         new ContactsModel
                         {
                              Email = physio.Email,
                               Name = physio.FullName
                         }
                     },
                    EmailDisplayName = "Health Enforcer",
                    Subject = $"Connection Notification from {request.PatientName}",
                    Message = connectionMessage[1]
                });
            }
            return Ok(result);
        }

        [HttpPost("editconnection")]
        public async Task<IActionResult> UpdateConnection(ConnectionRequestDTO request)
        {
            var result = await _userService.PatientConnectStatus(request);
            if (result.Status)
            {
                var updatePatientTherapist = JsonConvert.DeserializeObject<ConnectionRequestDTO>(JsonConvert.SerializeObject(result.ReturnObj));
                //send notification
                
                if(updatePatientTherapist.ConnectionStatus != ConnectionStatus.disconnected)
                {
                    var ownerIds = new List<Guid>() { request.PatientID};
                    await _mediatR.Send(NotificationHelper.GetNotificationModelManyOwnersOneMessage(ownerIds, NotificationHelper.GetNotificationMessage(updatePatientTherapist).Split('|')[0]));
                }

                if (updatePatientTherapist.ConnectionStatus == ConnectionStatus.disconnected)
                {
                    var connectionMsg = NotificationHelper.GetNotificationMessage(updatePatientTherapist).Split('|');
                    await _mediatR.Send(NotificationHelper.GetNotificationModelManyOwnersManyMessages(new List<NotificationRequester>()
                {
                    new NotificationRequester{ Message = connectionMsg[0], OwnerID = request.PatientID},
                    new NotificationRequester{ Message = connectionMsg[1], OwnerID = request.PhysiotherapistID}
                }));
                }

            }
            return Ok(result);
        }

        [HttpGet("mythetapists")]
        public IActionResult GetMyPhysiotherapists(Guid patientId) => Ok(_userService.MyPhysiotherapists(patientId));


       
    }
}
