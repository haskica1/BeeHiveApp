using BeeHive.Application.Features.Profile.DTOs;

namespace BeeHive.Application.Features.Profile;

public interface IProfileService
{
    Task<ProfileResponseDto> GetProfileAsync();
    Task<ProfileResponseDto> UpdateProfileAsync(UpdateProfileDto dto);
}
