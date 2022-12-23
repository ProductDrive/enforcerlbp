using AutoMapper;
using DTOs.RequestObject;
using DTOs.ResponseObject;
using Entities;
using Entities.Users;
using Infrastructures.NotificationService;

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
            CreateMap<Notification, NotificationSenderCommand>();
            CreateMap<NotificationSenderCommand, Notification>();
            CreateMap<ExercisePrescription, ExercisePrescriptionDTO>();
            CreateMap<ExercisePrescriptionDTO, ExercisePrescription>();

        }
    }
}
