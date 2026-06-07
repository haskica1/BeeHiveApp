using AutoMapper;
using BeeHive.Application.Features.Beehives.DTOs;
using BeeHive.Domain.Entities;

namespace BeeHive.Application.Features.Beehives;

public class BeehiveMappingProfile : AutoMapper.Profile
{
    public BeehiveMappingProfile()
    {
        CreateMap<Beehive, BeehiveDto>()
            .ForMember(d => d.TypeName, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.MaterialName, o => o.MapFrom(s => s.Material.ToString()))
            .ForMember(d => d.InspectionCount, o => o.MapFrom(s => s.Inspections.Count))
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s =>
                s.CreatedBy != null ? $"{s.CreatedBy.FirstName} {s.CreatedBy.LastName}" : null));

        CreateMap<Beehive, BeehiveDetailDto>()
            .ForMember(d => d.TypeName, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.MaterialName, o => o.MapFrom(s => s.Material.ToString()))
            .ForMember(d => d.InspectionCount, o => o.MapFrom(s => s.Inspections.Count()))
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s =>
                s.CreatedBy != null ? $"{s.CreatedBy.FirstName} {s.CreatedBy.LastName}" : null));

        CreateMap<CreateBeehiveDto, Beehive>();
        CreateMap<UpdateBeehiveDto, Beehive>();
    }
}
