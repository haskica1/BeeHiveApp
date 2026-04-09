using BeeHive.Application.Features.Apiaries.DTOs;
using BeeHive.Application.Features.Beehives.DTOs;
using BeeHive.Application.Features.Inspections.DTOs;
using BeeHive.Domain.Enums;
using FluentValidation;

namespace BeeHive.Application.Common.Behaviors;

// ── Apiary Validators ────────────────────────────────────────────────────────

public class CreateApiaryValidator : AbstractValidator<CreateApiaryDto>
{
    public CreateApiaryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Apiary name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => x.Description is not null);
    }
}

public class UpdateApiaryValidator : AbstractValidator<UpdateApiaryDto>
{
    public UpdateApiaryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Apiary name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => x.Description is not null);
    }
}

// ── Beehive Validators ───────────────────────────────────────────────────────

public class CreateBeehiveValidator : AbstractValidator<CreateBeehiveDto>
{
    public CreateBeehiveValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Beehive name/label is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid beehive type.");

        RuleFor(x => x.Material)
            .IsInEnum().WithMessage("Invalid beehive material.");

        RuleFor(x => x.DateCreated)
            .NotEmpty().WithMessage("Date created is required.")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Date created cannot be in the future.");

        RuleFor(x => x.ApiaryId)
            .GreaterThan(0).WithMessage("A valid apiary must be specified.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.")
            .When(x => x.Notes is not null);
    }
}

public class UpdateBeehiveValidator : AbstractValidator<UpdateBeehiveDto>
{
    public UpdateBeehiveValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Beehive name/label is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Type).IsInEnum().WithMessage("Invalid beehive type.");
        RuleFor(x => x.Material).IsInEnum().WithMessage("Invalid beehive material.");

        RuleFor(x => x.DateCreated)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Date created cannot be in the future.");

        RuleFor(x => x.ApiaryId).GreaterThan(0).WithMessage("A valid apiary must be specified.");
    }
}

// ── Inspection Validators ────────────────────────────────────────────────────

public class CreateInspectionValidator : AbstractValidator<CreateInspectionDto>
{
    public CreateInspectionValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Inspection date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Inspection date cannot be in the future.");

        RuleFor(x => x.Temperature)
            .InclusiveBetween(-50, 60).WithMessage("Temperature must be between -50 and 60 °C.")
            .When(x => x.Temperature.HasValue);

        RuleFor(x => x.HoneyLevel)
            .IsInEnum().WithMessage("Invalid honey level.");

        RuleFor(x => x.BeehiveId)
            .GreaterThan(0).WithMessage("A valid beehive must be specified.");

        RuleFor(x => x.BroodStatus)
            .MaximumLength(500).WithMessage("Brood status must not exceed 500 characters.")
            .When(x => x.BroodStatus is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.")
            .When(x => x.Notes is not null);
    }
}

public class UpdateInspectionValidator : AbstractValidator<UpdateInspectionDto>
{
    public UpdateInspectionValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Inspection date cannot be in the future.");

        RuleFor(x => x.Temperature)
            .InclusiveBetween(-50, 60).WithMessage("Temperature must be between -50 and 60 °C.")
            .When(x => x.Temperature.HasValue);

        RuleFor(x => x.HoneyLevel).IsInEnum().WithMessage("Invalid honey level.");
        RuleFor(x => x.BeehiveId).GreaterThan(0).WithMessage("A valid beehive must be specified.");
    }
}
