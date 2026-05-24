using BeeHive.Application.Features.Apiaries.DTOs;
using BeeHive.Application.Features.Beehives.DTOs;
using BeeHive.Application.Features.Diets.DTOs;
using BeeHive.Application.Features.Expenses.DTOs;
using BeeHive.Application.Features.Inspections.DTOs;
using BeeHive.Application.Features.Todos.DTOs;
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

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.")
            .When(x => x.Longitude.HasValue);
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

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.")
            .When(x => x.Longitude.HasValue);
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

// ── Todo Validators ──────────────────────────────────────────────────────────

public class CreateTodoValidator : AbstractValidator<CreateTodoDto>
{
    public CreateTodoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => x.Notes is not null);

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value.");

        RuleFor(x => x)
            .Must(x => x.ApiaryId.HasValue ^ x.BeehiveId.HasValue)
            .WithName("Target")
            .WithMessage("A todo must belong to exactly one apiary or one beehive.");

        RuleFor(x => x.ApiaryId)
            .GreaterThan(0).WithMessage("A valid apiary must be specified.")
            .When(x => x.ApiaryId.HasValue);

        RuleFor(x => x.BeehiveId)
            .GreaterThan(0).WithMessage("A valid beehive must be specified.")
            .When(x => x.BeehiveId.HasValue);

        RuleFor(x => x.AssignedToId)
            .GreaterThan(0).WithMessage("Invalid assignee.")
            .When(x => x.AssignedToId.HasValue);
    }
}

public class UpdateTodoValidator : AbstractValidator<UpdateTodoDto>
{
    public UpdateTodoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => x.Notes is not null);

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value.");

        RuleFor(x => x.AssignedToId)
            .GreaterThan(0).WithMessage("Invalid assignee.")
            .When(x => x.AssignedToId.HasValue);
    }
}

// ── Diet Validators ──────────────────────────────────────────────────────────

public class CreateDietValidator : AbstractValidator<CreateDietDto>
{
    public CreateDietValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Diet name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.Reason)
            .IsInEnum().WithMessage("Invalid diet reason.");

        RuleFor(x => x.CustomReason)
            .NotEmpty().WithMessage("Custom reason text is required when reason is Custom.")
            .MaximumLength(500)
            .When(x => x.Reason == DietReason.Custom);

        RuleFor(x => x.DurationDays)
            .GreaterThan(0).WithMessage("Duration must be at least 1 day.");

        RuleFor(x => x.FrequencyDays)
            .GreaterThan(0).WithMessage("Frequency must be at least 1 day.")
            .LessThanOrEqualTo(x => x.DurationDays).WithMessage("Frequency cannot exceed duration.");

        RuleFor(x => x.FoodType)
            .IsInEnum().WithMessage("Invalid food type.");

        RuleFor(x => x.CustomFoodType)
            .NotEmpty().WithMessage("Custom food type text is required when food type is Custom.")
            .MaximumLength(200)
            .When(x => x.FoodType == FoodType.Custom);

        RuleFor(x => x.BeehiveId)
            .GreaterThan(0).WithMessage("A valid beehive must be specified.");
    }
}

public class UpdateDietValidator : AbstractValidator<UpdateDietDto>
{
    public UpdateDietValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Diet name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.Reason)
            .IsInEnum().WithMessage("Invalid diet reason.");

        RuleFor(x => x.CustomReason)
            .NotEmpty().WithMessage("Custom reason text is required when reason is Custom.")
            .MaximumLength(500)
            .When(x => x.Reason == DietReason.Custom);

        RuleFor(x => x.DurationDays)
            .GreaterThan(0).WithMessage("Duration must be at least 1 day.");

        RuleFor(x => x.FrequencyDays)
            .GreaterThan(0).WithMessage("Frequency must be at least 1 day.")
            .LessThanOrEqualTo(x => x.DurationDays).WithMessage("Frequency cannot exceed duration.");

        RuleFor(x => x.FoodType)
            .IsInEnum().WithMessage("Invalid food type.");

        RuleFor(x => x.CustomFoodType)
            .NotEmpty().WithMessage("Custom food type text is required when food type is Custom.")
            .MaximumLength(200)
            .When(x => x.FoodType == FoodType.Custom);
    }
}

public class CompleteEarlyValidator : AbstractValidator<CompleteEarlyDto>
{
    public CompleteEarlyValidator()
    {
        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("A comment is required when stopping a diet early.")
            .MaximumLength(1000).WithMessage("Comment must not exceed 1000 characters.");
    }
}

// ── Expense Validators ───────────────────────────────────────────────────────

public class ExpenseItemValidator : AbstractValidator<CreateExpenseItemDto>
{
    public ExpenseItemValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price must be 0 or greater.");

        RuleFor(x => x.TotalPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Total price must be 0 or greater.");

        RuleFor(x => x.Unit)
            .MaximumLength(50).WithMessage("Unit label must not exceed 50 characters.")
            .When(x => x.Unit is not null);
    }
}

public class CreateExpenseValidator : AbstractValidator<CreateExpenseDto>
{
    public CreateExpenseValidator()
    {
        RuleFor(x => x.Source)
            .IsInEnum().WithMessage("Invalid expense source.");

        RuleFor(x => x.PurchaseDate)
            .NotEmpty().WithMessage("Purchase date is required.");

        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Total amount must be 0 or greater.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(10).WithMessage("Currency must not exceed 10 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.")
            .When(x => x.Notes is not null);

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one expense item is required.");

        RuleForEach(x => x.Items).SetValidator(new ExpenseItemValidator());
    }
}

public class UpdateExpenseValidator : AbstractValidator<UpdateExpenseDto>
{
    public UpdateExpenseValidator()
    {
        RuleFor(x => x.PurchaseDate)
            .NotEmpty().WithMessage("Purchase date is required.");

        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Total amount must be 0 or greater.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(10).WithMessage("Currency must not exceed 10 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.")
            .When(x => x.Notes is not null);

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one expense item is required.");

        RuleForEach(x => x.Items).SetValidator(new ExpenseItemValidator());
    }
}
