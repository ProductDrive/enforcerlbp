using AutoMapper;
using DataAccess.UnitOfWork;
using Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructures.NotificationService
{
    public class NotificationSenderHandler : IRequestHandler<NotificationSenderCommand, bool>
    {
        private readonly IUnitOfWork<Notification> _unitOfWorkNotification;
        private readonly IMapper _mapper;

        public NotificationSenderHandler(IUnitOfWork<Notification> unitOfWorkNotification, IMapper mapper)
        {
            _unitOfWorkNotification = unitOfWorkNotification;
            _mapper = mapper;
        }
        public async Task<bool> Handle(NotificationSenderCommand request, CancellationToken cancellationToken)
        {
            var notifications = new List<Notification>();
            foreach (var item in request.SenderCommands)
            {
                notifications.Add(_mapper.Map<Notification>(item));
            }
            
            try
            {
                
                await _unitOfWorkNotification.Repository.CreateMany(notifications);

                if (await _unitOfWorkNotification.Save() > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
