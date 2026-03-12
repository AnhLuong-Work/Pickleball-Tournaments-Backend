namespace AppPickleball.Application.Features.Auth.DTOs;

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    UserTokenDto User,
    bool IsNewUser = false
);

public record UserTokenDto(
    Guid Id,
    string Email,
    string Name,
    string? AvatarUrl,
    decimal SkillLevel,
    bool EmailVerified,
    DateTime CreatedAt
);
