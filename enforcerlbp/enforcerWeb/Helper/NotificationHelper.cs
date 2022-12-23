using DTOs.RequestObject;
using Entities.Users;
using Infrastructures.NotificationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace enforcerWeb.Helper
{
    public class NotificationHelper
    {
        public static string GetNotificationMessage(ConnectionRequestDTO updatePatientTherapist)
        {
            string message = string.Empty;
            switch (updatePatientTherapist.ConnectionStatus)
            {
                case ConnectionStatus.sent:
                    message = $"Notification of your connection has been sent to the physiotherapist|Hello,{updatePatientTherapist.PatientName} likes your services and will like to connect with you";
                    break;
                case ConnectionStatus.accepted:
                    message = $"Your connection request has been accepted by {updatePatientTherapist.TherapistName}";
                    break;
                case ConnectionStatus.rejected:
                    message = $"Your connection request has been declined by {updatePatientTherapist.TherapistName}";
                    break;
                case ConnectionStatus.disconnected:
                    message = $"You are no longer a connection with {updatePatientTherapist.PatientName}|You are no longer a connection with {updatePatientTherapist.TherapistName}";
                    break;
                default:
                    break;
            }
            return message;
        }

        public static NotificationSenderCommand GetNotificationModelManyOwnersOneMessage(List<Guid> OwnerIds, string message)
        {
            var command = new NotificationSenderCommand() { SenderCommands = new List<NotificationCommand>() };

            foreach (var ownerId in OwnerIds)
            {
                command.SenderCommands.Add(new NotificationCommand { OwnerId = ownerId, Message = message });
            }

            return command;
        }

        public static NotificationSenderCommand GetNotificationModelManyOwnersManyMessages(List<NotificationRequester> requesters)
        {
            var command = new NotificationSenderCommand() { SenderCommands = new List<NotificationCommand>() };

            foreach (var requester in requesters)
            {
                command.SenderCommands.Add(new NotificationCommand { OwnerId = requester.OwnerID, Message = requester.Message });
            }

            return command;
        }
    }
}
