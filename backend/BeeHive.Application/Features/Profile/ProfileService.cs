using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Features.Profile.DTOs;

namespace BeeHive.Application.Features.Profile;

public interface IProfileService
{
    Task<ProfileResponseDto> GetProfileAsync(int userId);
    Task<ProfileResponseDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
}

public class ProfileService : IProfileService
{
    private readonly IUnitOfWork _uow;

    public ProfileService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ProfileResponseDto> GetProfileAsync(int userId)
    {
        var user = await _uow.Users.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        return new ProfileResponseDto(user.FirstName, user.LastName, user.Email);
    }

    public async Task<ProfileResponseDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _uow.Users.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        // Email uniqueness check
        var newEmail = dto.Email.Trim().ToLower();
        if (!string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
        {
            var conflict = await _uow.Users.GetByEmailAsync(newEmail);
            if (conflict != null)
                throw new BusinessRuleException($"Email '{dto.Email}' is already in use.");
        }

        // Password change (optional)
        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                throw new BusinessRuleException("Current password is required to set a new password.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new BusinessRuleException("Current password is incorrect.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        }

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.Email = newEmail;

        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();

        return new ProfileResponseDto(user.FirstName, user.LastName, user.Email);
    }
}
