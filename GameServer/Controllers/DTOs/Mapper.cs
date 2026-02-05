using AutoMapper;
using GameServer.Auth.Models;
using Microsoft.AspNetCore.Identity;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<GuestDto,UserProfile>().ReverseMap();
    }
}