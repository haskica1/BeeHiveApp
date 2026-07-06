using BeeHive.Application.Features.Inspections.DTOs;
using FluentValidation;

namespace BeeHive.Application.Features.Inspections.Validators;

public class UploadInspectionPhotoValidator : AbstractValidator<UploadInspectionPhotoDto>
{
    public UploadInspectionPhotoValidator()
    {
        RuleFor(x => x.Caption)
            .MaximumLength(200).WithMessage("Opis fotografije ne smije prelaziti 200 znakova.");
    }
}
