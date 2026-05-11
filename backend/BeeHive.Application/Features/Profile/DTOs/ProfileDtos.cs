namespace BeeHive.Application.Features.Profile.DTOs;

public record UpdateProfileDto(
    string FirstName,
    string LastName,
    string Email,
    string? CurrentPassword,
    string? NewPassword
);

public record ProfileResponseDto(
    string FirstName,
    string LastName,
    string Email
);
