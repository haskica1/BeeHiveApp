using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Features.Auth.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BeeHive.Application.Features.Auth;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
}

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _config;

    public AuthService(IUnitOfWork uow, IConfiguration config)
    {
        _uow = uow;
        _config = config;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _uow.Users.GetByEmailAsync(dto.Email.Trim().ToLower())
            ?? throw new BusinessRuleException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new BusinessRuleException("Invalid email or password.");

        var token = GenerateToken(user.Id, user.Email, user.Role.ToString(), user.OrganizationId, user.ApiaryId);

        IReadOnlyList<int> assignedBeehiveIds = user.Role == Domain.Enums.UserRole.User
            ? (await _uow.Users.GetByIdWithAssignedBeehivesAsync(user.Id))
                ?.AssignedBeehives.Select(ub => ub.BeehiveId).ToList()
              ?? []
            : [];

        return new LoginResponseDto(
            Token: token,
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Role: user.Role.ToString(),
            OrganizationId: user.OrganizationId,
            OrganizationName: user.Organization?.Name,
            AssignedBeehiveIds: assignedBeehiveIds
        );
    }

    private string GenerateToken(int userId, string email, string role, int? organizationId, int? apiaryId)
    {
        var secret = _config["Jwt:Secret"]!;
        var issuer = _config["Jwt:Issuer"]!;
        var audience = _config["Jwt:Audience"]!;
        var expiryMinutes = int.Parse(_config["Jwt:ExpiryMinutes"]!);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claimsList = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (organizationId.HasValue)
            claimsList.Add(new Claim("organizationId", organizationId.Value.ToString()));

        if (apiaryId.HasValue)
            claimsList.Add(new Claim("apiaryId", apiaryId.Value.ToString()));

        var claims = claimsList.ToArray();

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
