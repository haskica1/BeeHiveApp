using BeeHive.Application.Features.Beehives.DTOs;
using FluentValidation;

namespace BeeHive.Application.Features.Beehives.Validators;

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
