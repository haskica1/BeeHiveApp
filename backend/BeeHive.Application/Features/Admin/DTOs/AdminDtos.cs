namespace BeeHive.Application.Features.Admin.DTOs;

// ── Organization DTOs ─────────────────────────────────────────────────────────

public class AdminOrganizationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UserCount { get; set; }
    public int ApiaryCount { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateOrganizationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateOrganizationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

// ── User DTOs ─────────────────────────────────────────────────────────────────

public class AdminUserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public int? ApiaryId { get; set; }
    public string? ApiaryName { get; set; }
    public List<int> AssignedBeehiveIds { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

public class CreateAdminUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Admin";
    public int? OrganizationId { get; set; }
    public int? ApiaryId { get; set; }
    public List<int> AssignedBeehiveIds { get; set; } = [];
}

public class UpdateAdminUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Admin";
    public int? OrganizationId { get; set; }
    public int? ApiaryId { get; set; }
    public List<int> AssignedBeehiveIds { get; set; } = [];
}

// ── Apiary list DTO (for org-scoped apiary picker in admin UI) ────────────────

public class AdminApiaryListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// ── Beehive list DTO (for org-scoped beehive picker in admin UI) ──────────────

public class AdminBeehiveListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApiaryName { get; set; } = string.Empty;
}
