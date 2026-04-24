using AutoMapper;
using BeeHive.Application.Features.Apiaries.DTOs;
using BeeHive.Application.Features.Beehives.DTOs;
using BeeHive.Application.Features.Inspections.DTOs;
using BeeHive.Application.Features.Todos.DTOs;
using BeeHive.Domain.Entities;

namespace BeeHive.Application.Common.Mappings;

/// <summary>AutoMapper profile containing all entity ↔ DTO mappings.</summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── Apiary ───────────────────────────────────────────────────────────
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

        // ── Beehive ──────────────────────────────────────────────────────────
        CreateMap<Beehive, BeehiveDto>()
            .ForMember(d => d.TypeName, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.MaterialName, o => o.MapFrom(s => s.Material.ToString()))
            .ForMember(d => d.InspectionCount, o => o.MapFrom(s => s.Inspections.Count))
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s =>
                s.CreatedBy != null ? $"{s.CreatedBy.FirstName} {s.CreatedBy.LastName}" : null));

        CreateMap<Beehive, BeehiveDetailDto>()
            .ForMember(d => d.TypeName, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.MaterialName, o => o.MapFrom(s => s.Material.ToString()))
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s =>
                s.CreatedBy != null ? $"{s.CreatedBy.FirstName} {s.CreatedBy.LastName}" : null));

        CreateMap<CreateBeehiveDto, Beehive>();
        CreateMap<UpdateBeehiveDto, Beehive>();

        // ── Inspection ───────────────────────────────────────────────────────
        CreateMap<Inspection, InspectionDto>()
            .ForMember(d => d.HoneyLevelName, o => o.MapFrom(s => s.HoneyLevel.ToString()));

        CreateMap<CreateInspectionDto, Inspection>();
        CreateMap<UpdateInspectionDto, Inspection>();

        // ── Todo ─────────────────────────────────────────────────────────────
        CreateMap<Todo, TodoDto>()
            .ForMember(d => d.PriorityName, o => o.MapFrom(s => s.Priority.ToString()))
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s =>
                s.CreatedBy != null ? $"{s.CreatedBy.FirstName} {s.CreatedBy.LastName}" : null))
            .ForMember(d => d.AssignedToName, o => o.MapFrom(s =>
                s.AssignedTo != null ? $"{s.AssignedTo.FirstName} {s.AssignedTo.LastName}" : null));

        CreateMap<CreateTodoDto, Todo>()
            .ForMember(d => d.AssignedTo, o => o.Ignore()); // navigation loaded separately
        CreateMap<UpdateTodoDto, Todo>()
            .ForMember(d => d.CompletedAt, o => o.Ignore()) // managed in service
            .ForMember(d => d.AssignedTo, o => o.Ignore());
    }
}
