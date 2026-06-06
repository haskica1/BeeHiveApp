using BeeHive.Application.Features.Auth.DTOs;

namespace BeeHive.Application.Features.Auth;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
}
