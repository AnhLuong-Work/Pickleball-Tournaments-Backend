using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Common.Settings;
using AppPickleball.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.Extensions.Options;
using Shared.Kernel.Wrappers;
using RefreshTokenEntity = AppPickleball.Domain.Entities.RefreshToken;

namespace AppPickleball.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ApiResponse<TokenResponseDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwtService;
    private readonly ICurrentUserService _currentUser;
    private readonly AuthSettings _authSettings;

    public ChangePasswordCommandHandler(IUserRepository userRepo, IRefreshTokenRepository refreshTokenRepo,
        IUnitOfWork uow, IPasswordHasher hasher, IJwtService jwtService,
        ICurrentUserService currentUser, IOptions<AuthSettings> authSettings)
    {
        _userRepo = userRepo; _refreshTokenRepo = refreshTokenRepo;
        _uow = uow; _hasher = hasher; _jwtService = jwtService;
        _currentUser = currentUser; _authSettings = authSettings.Value;
    }

    public async Task<ApiResponse<TokenResponseDto>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("User không tồn tại");

        if (user.PasswordHash == null)
            throw new DomainException("Tài khoản đăng nhập qua mạng xã hội không có mật khẩu");

        if (!_hasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            throw new DomainException("Mật khẩu hiện tại không đúng");

        if (_hasher.VerifyPassword(request.NewPassword, user.PasswordHash))
            throw new DomainException("Mật khẩu mới không được trùng mật khẩu cũ");

        user.PasswordHash = _hasher.HashPassword(request.NewPassword);
        _userRepo.Update(user);

        // Revoke all refresh tokens except device re-login
        await _refreshTokenRepo.RevokeAllUserTokensAsync(user.Id, cancellationToken);

        // Create new token
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
        return ApiResponse<TokenResponseDto>.SuccessResponse(
            new TokenResponseDto(accessToken, rawToken, _authSettings.AccessTokenExpiryMinutes * 60),
            "Đổi mật khẩu thành công");
    }
}
