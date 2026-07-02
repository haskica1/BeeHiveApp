using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Common.Localization;
using BeeHive.Application.Common.Security;
using BeeHive.Application.Features.Queens.DTOs;
using BeeHive.Domain.Common;
using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;

namespace BeeHive.Application.Features.Queens;

public class QueenService : IQueenService
{
    private readonly IUnitOfWork _uow;
    private readonly IAccessGuard _access;

    public QueenService(IUnitOfWork uow, IAccessGuard access)
    {
        _uow = uow;
        _access = access;
    }

    public async Task<IEnumerable<QueenDto>> GetByBeehiveIdAsync(int beehiveId)
    {
        if (!await _uow.Beehives.ExistsAsync(beehiveId))
            throw new NotFoundException(nameof(Beehive), beehiveId);

        await _access.EnsureCanAccessBeehiveAsync(beehiveId);

        var queens = await _uow.Queens.GetByBeehiveIdAsync(beehiveId);
        return queens.Select(ToDto);
    }

    public async Task<QueenDto> CreateAsync(int beehiveId, CreateQueenDto dto)
    {
        if (!await _uow.Beehives.ExistsAsync(beehiveId))
            throw new NotFoundException(nameof(Beehive), beehiveId);

        await _access.EnsureCanAccessBeehiveAsync(beehiveId);

        var queen = new Queen
        {
            BeehiveId      = beehiveId,
            Year           = dto.Year,
            MarkColor      = dto.MarkColor ?? QueenMarkColorHelper.ForYear(dto.Year),
            IsMarked       = dto.IsMarked,
            IsClipped      = dto.IsClipped,
            Origin         = dto.Origin,
            Status         = QueenStatus.Active,
            IntroducedDate = dto.IntroducedDate,
            Notes          = dto.Notes,
        };

        // Replacing: the previous active queen is closed in the same SaveChanges (atomic).
        var current = await _uow.Queens.GetActiveByBeehiveIdAsync(beehiveId);
        if (current is not null)
        {
            current.Status  = QueenStatus.Replaced;
            current.EndDate = dto.IntroducedDate;
            await _uow.Queens.UpdateAsync(current);
        }

        await _uow.Queens.AddAsync(queen);
        await _uow.SaveChangesAsync();

        return ToDto(queen);
    }

    public async Task<QueenDto> UpdateAsync(int id, UpdateQueenDto dto)
    {
        var queen = await _uow.Queens.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Queen), id);

        await _access.EnsureCanAccessBeehiveAsync(queen.BeehiveId);

        if (dto.Status == QueenStatus.Active)
        {
            var active = await _uow.Queens.GetActiveByBeehiveIdAsync(queen.BeehiveId);
            if (active is not null && active.Id != queen.Id)
                throw new BusinessRuleException("The beehive already has an active queen — close it first.");
        }

        queen.Year           = dto.Year;
        queen.MarkColor      = dto.MarkColor;
        queen.IsMarked       = dto.IsMarked;
        queen.IsClipped      = dto.IsClipped;
        queen.Origin         = dto.Origin;
        queen.Status         = dto.Status;
        queen.IntroducedDate = dto.IntroducedDate;
        queen.Notes          = dto.Notes;
        queen.EndDate        = dto.Status == QueenStatus.Active
            ? null
            : dto.EndDate ?? queen.EndDate ?? DateTime.UtcNow;

        await _uow.Queens.UpdateAsync(queen);
        await _uow.SaveChangesAsync();

        return ToDto(queen);
    }

    public async Task DeleteAsync(int id)
    {
        var queen = await _uow.Queens.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Queen), id);

        await _access.EnsureCanAccessBeehiveAsync(queen.BeehiveId);

        await _uow.Queens.DeleteAsync(queen);
        await _uow.SaveChangesAsync();
    }

    // Manual mapping — the DTO carries computed Bosnian *Name fields (same policy as Diets/Admin).
    private static QueenDto ToDto(Queen q) => new()
    {
        Id             = q.Id,
        Year           = q.Year,
        MarkColor      = q.MarkColor,
        MarkColorName  = BsLabels.Label(q.MarkColor),
        IsMarked       = q.IsMarked,
        IsClipped      = q.IsClipped,
        Origin         = q.Origin,
        OriginName     = BsLabels.Label(q.Origin),
        Status         = q.Status,
        StatusName     = BsLabels.Label(q.Status),
        IntroducedDate = q.IntroducedDate,
        EndDate        = q.EndDate,
        Notes          = q.Notes,
        BeehiveId      = q.BeehiveId,
        CreatedAt      = q.CreatedAt,
    };
}
