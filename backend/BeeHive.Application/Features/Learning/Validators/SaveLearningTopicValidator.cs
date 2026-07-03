using BeeHive.Application.Features.Learning.DTOs;
using FluentValidation;

namespace BeeHive.Application.Features.Learning.Validators;

public class SaveLearningTopicValidator : AbstractValidator<SaveLearningTopicDto>
{
    public SaveLearningTopicValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Naslov je obavezan.")
            .MaximumLength(150).WithMessage("Naslov može imati najviše 150 znakova.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Nepoznata kategorija.");

        RuleFor(x => x.Summary)
            .NotEmpty().WithMessage("Sažetak je obavezan.")
            .MaximumLength(300).WithMessage("Sažetak može imati najviše 300 znakova.");

        // A draft may have an empty body; publish enforces content in the service.
        RuleForEach(x => x.Months)
            .InclusiveBetween(1, 12).WithMessage("Mjeseci moraju biti između 1 i 12.")
            .When(x => x.Months is not null);
    }
}
