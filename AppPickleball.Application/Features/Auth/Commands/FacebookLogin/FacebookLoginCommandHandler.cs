using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Settings;
using AppPickleball.Application.Features.Auth.DTOs;
using AppPickleball.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;
using Shared.Kernel.Wrappers;
using UserEntity = AppPickleball.Domain.Entities.User;
using RefreshTokenEntity = AppPickleball.Domain.Entities.RefreshToken;
using AppPickleball.Application.Common.Services;

namespace AppPickleball.Application.Features.Auth.Commands.FacebookLogin;

public class FacebookLoginCommandHandler : IRequestHandler<FacebookLoginCommand, ApiResponse<AuthResponseDto>>
{
    private readonly IFacebookAuthService _facebookAuth;
    private readonly IUserRepository _userRepo;
    private readonly IUserAuthProviderRepository _authProviderRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwtService;
    private readonly AuthSettings _authSettings;

    public FacebookLoginCommandHandler(
        IFacebookAuthService facebookAuth,
        IUserRepository userRepo,
        IUserAuthProviderRepository authProviderRepo,
        IRefreshTokenRepository refreshTokenRepo,
        IUnitOfWork uow,
        IJwtService jwtService,
        IOptions<AuthSettings> authSettings)
    {
        _facebookAuth = facebookAuth;
        _userRepo = userRepo;
        _authProviderRepo = authProviderRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _uow = uow;
        _jwtService = jwtService;
        _authSettings = authSettings.Value;
    }

    public async Task<ApiResponse<AuthResponseDto>> Handle(FacebookLoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify Access Token với Facebook Graph API
        var fbUser = await _facebookAuth.VerifyAccessTokenAsync(request.AccessToken, cancellationToken);

        // 2. Find/Create User
        var existingProvider = await _authProviderRepo.FindByProviderAsync("facebook", fbUser.FacebookId, cancellationToken);

        UserEntity user;
        bool isNewUser = false;

        if (existingProvider != null)
        {
            // Case A: Đã link Facebook → Đăng nhập nhanh
            var linked = await _userRepo.GetByIdAsync(existingProvider.UserId, cancellationToken);
            if (linked == null) throw new NotFoundException("User không tồn tại");
            user = linked;
        }
        else if (!string.IsNullOrEmpty(fbUser.Email))
        {
            var existingUser = await _userRepo.GetByEmailAsync(fbUser.Email.ToLower(), cancellationToken);

            if (existingUser != null)
            {
                // Case B: Email đã tồn tại, chưa link Facebook → Link thêm
                user = existingUser;
                await _authProviderRepo.AddAsync(new UserAuthProvider
                {
                    UserId = user.Id,
                    Provider = "facebook",
                    ProviderUserId = fbUser.FacebookId,
                    Email = fbUser.Email,
                    Name = fbUser.Name
                }, cancellationToken);
            }
            else
            {
                // Case C: User hoàn toàn mới (có email)
                isNewUser = true;
                user = new UserEntity
                {
                    Email = fbUser.Email.ToLower(),
                    Name = fbUser.Name ?? fbUser.Email,
                    EmailVerified = true,
                    EmailVerifiedAt = DateTime.UtcNow,
                    PasswordHash = null
                };
                await _userRepo.AddAsync(user, cancellationToken);
                await _authProviderRepo.AddAsync(new UserAuthProvider
                {
                    UserId = user.Id,
                    Provider = "facebook",
                    ProviderUserId = fbUser.FacebookId,
                    Email = fbUser.Email,
                    Name = fbUser.Name
                }, cancellationToken);
            }
        }
        else
        {
            // Case D: Không có email → Tạo user với placeholder email
            isNewUser = true;
            user = new UserEntity
            {
                Email = $"fb_{fbUser.FacebookId}@facebook.placeholder",
                Name = fbUser.Name ?? $"Facebook User {fbUser.FacebookId}",
                EmailVerified = false,
                PasswordHash = null
            };
            await _userRepo.AddAsync(user, cancellationToken);
            await _authProviderRepo.AddAsync(new UserAuthProvider
            {
                UserId = user.Id,
                Provider = "facebook",
                ProviderUserId = fbUser.FacebookId,
                Email = null,
                Name = fbUser.Name
            }, cancellationToken);
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

        return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Đăng nhập Facebook thành công");
    }

    private static UserTokenDto MapUser(UserEntity u) =>
        new(u.Id, u.Email, u.Name, u.AvatarUrl, u.SkillLevel, u.EmailVerified, u.CreatedAt);
}
