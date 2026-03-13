using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Common.Settings;
using AppPickleball.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using Shared.Kernel.Resources;
using Shared.Kernel.Wrappers;
using UserEntity = AppPickleball.Domain.Entities.User;
using RefreshTokenEntity = AppPickleball.Domain.Entities.RefreshToken;

namespace AppPickleball.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<AuthResponseDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwtService;
    private readonly AuthSettings _authSettings;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public LoginCommandHandler(
        IUserRepository userRepo, IRefreshTokenRepository refreshTokenRepo,
        IUnitOfWork uow, IPasswordHasher hasher, IJwtService jwtService,
        IOptions<AuthSettings> authSettings, IStringLocalizer<SharedResource> localizer)
    {
        _userRepo = userRepo; _refreshTokenRepo = refreshTokenRepo;
        _uow = uow; _hasher = hasher; _jwtService = jwtService;
        _authSettings = authSettings.Value;
        _localizer = localizer;
    }

    public async Task<ApiResponse<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email.ToLower().Trim(), cancellationToken);
        if (user == null || user.PasswordHash == null || !_hasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Email hoặc mật khẩu không đúng");

        var rawToken = _jwtService.GenerateRefreshToken();
        var refreshToken = new RefreshTokenEntity
        {
            UserId = user.Id,
            TokenHash = _jwtService.HashToken(rawToken),
            ExpiresAt = DateTime.UtcNow.AddDays(_authSettings.RefreshTokenExpiryDays)
        };
        await _refreshTokenRepo.AddAsync(refreshToken, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtService.GenerateAccessToken(user);
        var response = new AuthResponseDto(accessToken, rawToken, _authSettings.AccessTokenExpiryMinutes * 60, MapUser(user));
        return ApiResponse<AuthResponseDto>.SuccessResponse(response, _localizer["Login_Success"]);
    }

    private static UserTokenDto MapUser(UserEntity u) => new(u.Id, u.Email, u.Name, u.AvatarUrl, u.SkillLevel, u.EmailVerified, u.CreatedAt);
}
