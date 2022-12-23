using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructures.NotificationService
{
    public class NotificationSenderCommand : IRequest<bool>
    {
        public List<NotificationCommand> SenderCommands { get; set; }
    }

    public class NotificationCommand
    {
        public Guid ID { get; set; }
        public Guid OwnerId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
    }
}
