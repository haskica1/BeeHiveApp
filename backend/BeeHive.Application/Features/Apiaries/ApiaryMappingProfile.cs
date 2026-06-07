using AutoMapper;
using BeeHive.Application.Features.Apiaries.DTOs;
using BeeHive.Domain.Entities;

namespace BeeHive.Application.Features.Apiaries;

public class ApiaryMappingProfile : AutoMapper.Profile
{
    public ApiaryMappingProfile()
    {
        CreateMap<Apiary, ApiaryDto>()
            .ForMember(d => d.BeehiveCount, o => o.MapFrom(s => s.Beehives.Count))
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s =>
                s.CreatedBy != null ? $"{s.CreatedBy.FirstName} {s.CreatedBy.LastName}" : null));

        CreateMap<Apiary, ApiaryDetailDto>()
            .ForMember(d => d.BeehiveCount, o => o.MapFrom(s => s.Beehives.Count))
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s =>
                s.CreatedBy != null ? $"{s.CreatedBy.FirstName} {s.CreatedBy.LastName}" : null));

        CreateMap<CreateApiaryDto, Apiary>();
        CreateMap<UpdateApiaryDto, Apiary>();
    }
}
