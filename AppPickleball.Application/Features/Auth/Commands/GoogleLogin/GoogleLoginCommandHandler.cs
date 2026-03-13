using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Settings;
using AppPickleball.Application.Features.Auth.DTOs;
using AppPickleball.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using Shared.Kernel.Resources;
using Shared.Kernel.Wrappers;
using UserEntity = AppPickleball.Domain.Entities.User;
using RefreshTokenEntity = AppPickleball.Domain.Entities.RefreshToken;
using AppPickleball.Application.Common.Services;

namespace AppPickleball.Application.Features.Auth.Commands.GoogleLogin;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, ApiResponse<AuthResponseDto>>
{
    private readonly IGoogleAuthService _googleAuth;
    private readonly IUserRepository _userRepo;
    private readonly IUserAuthProviderRepository _authProviderRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwtService;
    private readonly AuthSettings _authSettings;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public GoogleLoginCommandHandler(
        IGoogleAuthService googleAuth,
        IUserRepository userRepo,
        IUserAuthProviderRepository authProviderRepo,
        IRefreshTokenRepository refreshTokenRepo,
        IUnitOfWork uow,
        IJwtService jwtService,
        IOptions<AuthSettings> authSettings,
        IStringLocalizer<SharedResource> localizer)
    {
        _googleAuth = googleAuth;
        _userRepo = userRepo;
        _authProviderRepo = authProviderRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _uow = uow;
        _jwtService = jwtService;
        _authSettings = authSettings.Value;
        _localizer = localizer;
    }

    public async Task<ApiResponse<AuthResponseDto>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify ID Token với Google
        var googleUser = await _googleAuth.VerifyIdTokenAsync(request.IdToken, cancellationToken);

        // 2. Find/Create User
        var existingProvider = await _authProviderRepo.FindByProviderAsync("google", googleUser.GoogleId, cancellationToken);

        UserEntity user;
        bool isNewUser = false;

        if (existingProvider != null)
        {
            // Case A: Đã link Google → Đăng nhập nhanh
            var linked = await _userRepo.GetByIdAsync(existingProvider.UserId, cancellationToken);
            if (linked == null) throw new NotFoundException("User không tồn tại");
            user = linked;
        }
        else
        {
            var existingUser = await _userRepo.GetByEmailAsync(googleUser.Email.ToLower(), cancellationToken);

            if (existingUser != null)
            {
                // Case B: Email đã tồn tại, chưa link Google → Link thêm
                user = existingUser;
                await _authProviderRepo.AddAsync(new UserAuthProvider
                {
                    UserId = user.Id,
                    Provider = "google",
                    ProviderUserId = googleUser.GoogleId,
                    Email = googleUser.Email,
                    Name = googleUser.Name,
                    AvatarUrl = googleUser.Picture
                }, cancellationToken);
            }
            else
            {
                // Case C: User hoàn toàn mới → Tạo mới
                isNewUser = true;
                user = new UserEntity
                {
                    Email = googleUser.Email.ToLower(),
                    Name = googleUser.Name ?? googleUser.Email,
                    AvatarUrl = googleUser.Picture,
                    EmailVerified = true,
                    EmailVerifiedAt = DateTime.UtcNow,
                    PasswordHash = null
                };
                await _userRepo.AddAsync(user, cancellationToken);
                await _authProviderRepo.AddAsync(new UserAuthProvider
                {
                    UserId = user.Id,
                    Provider = "google",
                    ProviderUserId = googleUser.GoogleId,
                    Email = googleUser.Email,
                    Name = googleUser.Name,
                    AvatarUrl = googleUser.Picture
                }, cancellationToken);
            }
        }

        // 3. Generate JWT tokens
        var rawRefreshToken = _jwtService.GenerateRefreshToken();
        await _refreshTokenRepo.AddAsync(new RefreshTokenEntity
        {
            UserId = user.Id,
            TokenHash = _jwtService.HashToken(rawRefreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(_authSettings.RefreshTokenExpiryDays)
        }, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtService.GenerateAccessToken(user);
        var response = new AuthResponseDto(
            accessToken, rawRefreshToken,
            _authSettings.AccessTokenExpiryMinutes * 60,
            MapUser(user), isNewUser);

        return ApiResponse<AuthResponseDto>.SuccessResponse(response, _localizer["GoogleLogin_Success"]);
    }

    private static UserTokenDto MapUser(UserEntity u) =>
        new(u.Id, u.Email, u.Name, u.AvatarUrl, u.SkillLevel, u.EmailVerified, u.CreatedAt);
}
