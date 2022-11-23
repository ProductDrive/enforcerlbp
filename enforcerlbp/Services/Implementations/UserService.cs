using AutoMapper;
using DataAccess.UnitOfWork;
using DTOs.RequestObject;
using DTOs.ResponseObject;
using Entities.Users;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork<Physiotherapist> _unitOfWorkPhysio;
        private readonly IUnitOfWork<Patient> _unitOfWorkPatient;

        public IMapper _Mapper { get; }

        public UserService(
            IUnitOfWork<Physiotherapist> unitOfWorkPhysio,
            IUnitOfWork<Patient> unitOfWorkPatient,
            IMapper mapper)
        {
            _unitOfWorkPhysio = unitOfWorkPhysio;
            _unitOfWorkPatient = unitOfWorkPatient;
            _Mapper = mapper;
        }

        public async Task<ResponseModel> CreatePhysiotherapist(PhysiotherapistDTO model)
        {
            var physiotherapist = _Mapper.Map<PhysiotherapistDTO, Physiotherapist>(model);
            await _unitOfWorkPhysio.Repository.Create(physiotherapist);
            await _unitOfWorkPhysio.Save();
            return new ResponseModel{ Status=true,Response="Therapist created Successfully"};
        }
        public async Task<ResponseModel> CreatePatient(PatientDTO model)
        {
            var patient = _Mapper.Map<PatientDTO, Patient>(model);
            await _unitOfWorkPatient.Repository.Create(patient);
            await _unitOfWorkPatient.Save();
            return new ResponseModel { Status = true, Response = "Patient created Successfully" };
        }
    }
}
