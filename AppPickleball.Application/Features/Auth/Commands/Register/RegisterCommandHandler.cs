using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Auth.Interfaces;
using AppPickleball.Application.Features.Users.Interfaces;
using AppPickleball.Application.Common.Settings;
using AppPickleball.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using Shared.Kernel.Resources;
using Shared.Kernel.Wrappers;
using UserEntity = AppPickleball.Domain.Entities.User;
using RefreshTokenEntity = AppPickleball.Domain.Entities.RefreshToken;

namespace AppPickleball.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<AuthResponseDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwtService;
    private readonly AuthSettings _authSettings;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public RegisterCommandHandler(
        IUserRepository userRepo,
        IRefreshTokenRepository refreshTokenRepo,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        IJwtService jwtService,
        IOptions<AuthSettings> authSettings,
        IStringLocalizer<SharedResource> localizer)
    {
        _userRepo = userRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _uow = uow;
        _hasher = hasher;
        _jwtService = jwtService;
        _authSettings = authSettings.Value;
        _localizer = localizer;
    }

    public async Task<ApiResponse<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepo.EmailExistsAsync(request.Email, cancellationToken))
            throw new DomainException("Email đã được đăng ký");

        var user = new UserEntity
        {
            Email = request.Email.ToLower().Trim(),
            PasswordHash = _hasher.HashPassword(request.Password),
            Name = request.Name.Trim(),
            SkillLevel = 3.0m,
            EmailVerified = true
        };

        await _userRepo.AddAsync(user, cancellationToken);

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

        var response = new AuthResponseDto(
            AccessToken: accessToken,
            RefreshToken: rawToken,
            ExpiresIn: _authSettings.AccessTokenExpiryMinutes * 60,
            User: MapUser(user)
        );

        return ApiResponse<AuthResponseDto>.SuccessResponse(response, _localizer["Register_Success"], 201);
    }

    private static UserTokenDto MapUser(UserEntity u) => new(u.Id, u.Email, u.Name, u.AvatarUrl, u.SkillLevel, u.EmailVerified, u.CreatedAt);
}
