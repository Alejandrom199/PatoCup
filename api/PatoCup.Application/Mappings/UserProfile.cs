using AutoMapper;
using PatoCup.Application.DTOs.Security;
using PatoCup.Domain.Entities.Competition;
using PatoCup.Domain.Entities.Security;

namespace PatoCup.Application.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Mapeos de entrada
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.Password, opt => opt.Ignore());

            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.Password, opt => opt.Ignore());

            // Mapeo de salida
            CreateMap<User, UserResponseDto>();
        }
    }
}
