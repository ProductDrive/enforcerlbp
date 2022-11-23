using AutoMapper;
using DTOs.RequestObject;
using DTOs.ResponseObject;
using Entities.Users;

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


        }
    }
}
