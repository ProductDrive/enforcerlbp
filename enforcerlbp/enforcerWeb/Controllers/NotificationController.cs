using AutoMapper;
using DTOs.ResponseObject;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class NotificationController: ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediatR;
        private readonly IUserService _userService;
        public NotificationController(IMapper mapper, IMediator mediatR, IUserService userService)
        {
            _mapper = mapper;
            _mediatR = mediatR;
            _userService = userService;

        }



        [HttpGet]
        public ResponseModel GetAllNotification(Guid ownerId) => _userService.GetNotifications(ownerId);

        [HttpGet("pending")]
        public ResponseModel GetWaitingNotification(Guid ownerId) => _userService.GetPendingNotifications(ownerId);

        [HttpGet("seen")]
        public ResponseModel GetPreviousNotification(Guid ownerId) => _userService.GetSeenNotifications(ownerId);

        [HttpGet("active")]
        public int GetNotificationCount(Guid ownerId) => _userService.MyNotifications(ownerId);

        [HttpGet("markasread")]
        public async Task<ResponseModel> UpdateNotificationAsRead(Guid notificationId) => await _userService.ReadNotification(notificationId);

        [HttpDelete("markasdelete")]
        public async Task<ResponseModel> DeleteANotification(Guid notificationId) => await _userService.DeleteNotification(notificationId);
    }
}
