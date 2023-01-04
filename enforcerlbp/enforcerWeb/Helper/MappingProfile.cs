using AutoMapper;
using DTOs.RequestObject;
using DTOs.ResponseObject;
using Entities;
using Entities.Consultation;
using Entities.Users;
using Infrastructures.NotificationService;
using System.Collections.Generic;

namespace enforcerWeb.Helper
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<PhysiotherapistDTO, Physiotherapist>();
            CreateMap<Physiotherapist, PhysiotherapistDTO>();
            CreateMap<PatientDTO, Patient>();
            CreateMap<Patient, PatientDTO>();
            CreateMap<AppUserDTO, PhysiotherapistDTO>();
            CreateMap<AppUserDTO, PatientDTO>();
            CreateMap<ConnectionRequestDTO, PatientTherapist>();
            CreateMap<PatientTherapist, ConnectionRequestDTO>();
            CreateMap<Notification, NotificationCommand>();
            CreateMap<NotificationCommand, Notification>();
            CreateMap<ExercisePrescription, ExercisePrescriptionDTO>();
            CreateMap<ExercisePrescriptionDTO, ExercisePrescription>();
            CreateMap<ExerciseDTO, Exercise>();
            CreateMap<Exercise, ExerciseDTO>();
            CreateMap<Feedback, FeedbackRequestDTO>();
            CreateMap<FeedbackRequestDTO, Feedback>();
            CreateMap<FeedbackResponseDTO, Feedback>();
            CreateMap<Feedback, FeedbackResponseDTO>();
            CreateMap<FeedbackReply, FeedbackReplyDTO>();
            CreateMap<FeedbackReplyDTO, FeedbackReply>();
            CreateMap<PhysioSessionDTO, PhysioSession>();
            CreateMap<PhysioSession, PhysioSessionDTO>();
        }
    }
}
