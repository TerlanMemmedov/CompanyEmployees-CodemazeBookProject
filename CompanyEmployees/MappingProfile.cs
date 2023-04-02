using Entities.Models;
using Shared.DataTransferObjects;
using AutoMapper;

namespace CompanyEmployees
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            //this is the best for with get init records, so like to support xml
            //gives error ctor with no parameter in the just json, so we are use the one that below
            CreateMap<Company, CompanyDto>()
                .ForMember(c => c.FullAddress,
                opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));


            //this is the best for with no get init records
            /*
            CreateMap<Company, CompanyDto>()
                .ForCtorParam("FullAddress",
                    opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));
             */
                
            CreateMap<Employee, EmployeeDto>();

            CreateMap<CompanyForCreationDto, Company>();

            CreateMap<EmployeeForCreationDto, Employee>();

            CreateMap<EmployeeForUpdateDto, Employee>().ReverseMap();

            CreateMap<CompanyForUpdateDto, Company>();

            CreateMap<UserForRegistrationDto, User>();
         
        }
    }
}
