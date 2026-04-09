using AutoMapper;
using BeeHive.Application.Features.Apiaries.DTOs;
using BeeHive.Application.Features.Beehives.DTOs;
using BeeHive.Application.Features.Inspections.DTOs;
using BeeHive.Domain.Entities;

namespace BeeHive.Application.Common.Mappings;

/// <summary>AutoMapper profile containing all entity ↔ DTO mappings.</summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── Apiary ───────────────────────────────────────────────────────────
        CreateMap<Apiary, ApiaryDto>()
            .ForMember(d => d.BeehiveCount, o => o.MapFrom(s => s.Beehives.Count));

        CreateMap<Apiary, ApiaryDetailDto>()
            .ForMember(d => d.BeehiveCount, o => o.MapFrom(s => s.Beehives.Count));

        CreateMap<CreateApiaryDto, Apiary>();
        CreateMap<UpdateApiaryDto, Apiary>();

        // ── Beehive ──────────────────────────────────────────────────────────
        CreateMap<Beehive, BeehiveDto>()
            .ForMember(d => d.TypeName, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.MaterialName, o => o.MapFrom(s => s.Material.ToString()))
            .ForMember(d => d.InspectionCount, o => o.MapFrom(s => s.Inspections.Count));

        CreateMap<Beehive, BeehiveDetailDto>()
            .ForMember(d => d.TypeName, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.MaterialName, o => o.MapFrom(s => s.Material.ToString()));

        CreateMap<CreateBeehiveDto, Beehive>();
        CreateMap<UpdateBeehiveDto, Beehive>();

        // ── Inspection ───────────────────────────────────────────────────────
        CreateMap<Inspection, InspectionDto>()
            .ForMember(d => d.HoneyLevelName, o => o.MapFrom(s => s.HoneyLevel.ToString()));

        CreateMap<CreateInspectionDto, Inspection>();
        CreateMap<UpdateInspectionDto, Inspection>();
    }
}
