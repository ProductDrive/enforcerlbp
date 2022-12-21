using AutoMapper;
using DTOs.RequestObject;
using DTOs.ResponseObject;
using Entities.Users;
using Infrastructures.EmailServices;
using Infrastructures.NotificationService;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("sendconnection")]
        public async Task<ResponseModel> ConnectToATherapist(ConnectionRequestDTO request)
        {
            var result = await _userService.PatientConnectRequest(request);

            if (result.Status)
            {
                //send notification
                await _mediatR.Send(new NotificationSenderCommand
                {
                    SenderCommands = new List<NotificationCommand>()
                   {
                       new NotificationCommand
                       {
                            Message = "Notification of your connection has been sent to the physiotherapist",
                            OwnerId = request.PatientID
                       },
                       new NotificationCommand
                       {
                           Message = request.Message?? $"Hello,{request.PatientName} likes your services and will like to connect with you",
                           OwnerId = request.PhysiotherapistID
                       }
                   }
                });
                var query = await _userService.GetAPhysioTherapist(request.PhysiotherapistID);
                var physio = (PhysioTherapistConnectDTO)query.ReturnObj;
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
                    Subject = $"Connection Notification from {request.PatientName}"
                });
            }

            return result;
        }
    }
}
