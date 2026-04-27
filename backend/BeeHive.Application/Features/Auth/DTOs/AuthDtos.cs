namespace BeeHive.Application.Features.Auth.DTOs;

public record LoginDto(string Email, string Password);

public record LoginResponseDto(
    string Token,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    int? OrganizationId,
    string? OrganizationName,
    IReadOnlyList<int> AssignedBeehiveIds
);
