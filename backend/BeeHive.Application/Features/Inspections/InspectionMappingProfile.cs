using AutoMapper;
using BeeHive.Application.Features.Inspections.DTOs;
using BeeHive.Domain.Entities;

namespace BeeHive.Application.Features.Inspections;

public class InspectionMappingProfile : AutoMapper.Profile
{
    public InspectionMappingProfile()
    {
        CreateMap<Inspection, InspectionDto>()
            .ForMember(d => d.HoneyLevelName, o => o.MapFrom(s => s.HoneyLevel.ToString()));

        CreateMap<CreateInspectionDto, Inspection>();
        CreateMap<UpdateInspectionDto, Inspection>();
    }
}
