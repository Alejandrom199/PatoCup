using AutoMapper;
using PatoCup.Application.DTOs.Competition;
using PatoCup.Domain.Entities.Competition;
using System.Numerics;

public class PlayerProfile : Profile
{
    public PlayerProfile()
    {
        // Mapeos de entrada
        CreateMap<PublicSubmitPlayerDto, Player>(); 
        CreateMap<UpdatePlayerDto, Player>();

        // Mapeo de salida
        CreateMap<Player, PlayerResponseDto>();
        CreateMap<Player, PlayerSelectDto>();
    }
}