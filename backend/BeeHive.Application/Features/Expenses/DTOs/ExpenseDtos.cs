using BeeHive.Domain.Enums;

namespace BeeHive.Application.Features.Expenses.DTOs;

// ── Read DTOs ────────────────────────────────────────────────────────────────

public class ExpenseItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>Lightweight expense representation used in list views.</summary>
public class ExpenseDto
{
    public int Id { get; set; }
    public ExpenseSource Source { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "BAM";
    public string? Notes { get; set; }
    public int ItemCount { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Full expense representation including all line items.</summary>
public class ExpenseDetailDto : ExpenseDto
{
    public List<ExpenseItemDto> Items { get; set; } = [];
}

// ── Write DTOs ───────────────────────────────────────────────────────────────

public class CreateExpenseItemDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int SortOrder { get; set; }
}

public class CreateExpenseDto
{
    public ExpenseSource Source { get; set; } = ExpenseSource.Manual;
    public DateTime PurchaseDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "BAM";
    public string? Notes { get; set; }
    public List<CreateExpenseItemDto> Items { get; set; } = [];
}

public class UpdateExpenseDto
{
    public DateTime PurchaseDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "BAM";
    public string? Notes { get; set; }
    public List<CreateExpenseItemDto> Items { get; set; } = [];
}
