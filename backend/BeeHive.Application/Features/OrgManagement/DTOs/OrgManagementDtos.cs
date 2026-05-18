namespace BeeHive.Application.Features.OrgManagement.DTOs;

public class OrgMemberDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? ApiaryId { get; set; }
    public string? ApiaryName { get; set; }
    public List<int> AssignedBeehiveIds { get; set; } = [];
    public List<string> AssignedBeehiveNames { get; set; } = [];
}

public class UpdateBeehiveAssignmentsDto
{
    public List<int> BeehiveIds { get; set; } = [];
}

public class UpdateApiaryAssignmentDto
{
    public int? ApiaryId { get; set; }
}

public class OrgAvailableBeehiveDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApiaryName { get; set; } = string.Empty;
}

public class OrgAvailableApiaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
