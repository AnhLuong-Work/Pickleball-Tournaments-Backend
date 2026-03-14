using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Auth.Interfaces;
using AppPickleball.Application.Common.Settings;
using AppPickleball.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using Shared.Kernel.Resources;
using Shared.Kernel.Wrappers;
using RefreshTokenEntity = AppPickleball.Domain.Entities.RefreshToken;

namespace AppPickleball.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<TokenResponseDto>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwtService;
    private readonly AuthSettings _authSettings;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public RefreshTokenCommandHandler(IRefreshTokenRepository refreshTokenRepo, IUnitOfWork uow,
        IJwtService jwtService, IOptions<AuthSettings> authSettings, IStringLocalizer<SharedResource> localizer)
    {
        _refreshTokenRepo = refreshTokenRepo; _uow = uow;
        _jwtService = jwtService; _authSettings = authSettings.Value;
        _localizer = localizer;
    }

    public async Task<ApiResponse<TokenResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _jwtService.HashToken(request.RefreshToken);
        var existing = await _refreshTokenRepo.GetByTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new UnauthorizedException(_localizer["RefreshToken_Invalid"]);

        if (existing.IsRevoked)
        {
            // Reuse detected — revoke all tokens
            await _refreshTokenRepo.RevokeAllUserTokensAsync(existing.UserId, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException(_localizer["RefreshToken_Revoked"]);
        }

        if (existing.IsExpired)
            throw new UnauthorizedException(_localizer["RefreshToken_Expired"]);

        // Revoke old token
        existing.RevokedAt = DateTime.UtcNow;

        // Create new token pair
        var newRawToken = _jwtService.GenerateRefreshToken();
        var newRefreshToken = new RefreshTokenEntity
        {
            UserId = existing.UserId,
            TokenHash = _jwtService.HashToken(newRawToken),
            ExpiresAt = DateTime.UtcNow.AddDays(_authSettings.RefreshTokenExpiryDays)
        };

        existing.ReplacedByTokenId = newRefreshToken.Id;
        await _refreshTokenRepo.AddAsync(newRefreshToken, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtService.GenerateAccessToken(existing.User);
        return ApiResponse<TokenResponseDto>.SuccessResponse(
            new TokenResponseDto(accessToken, newRawToken, _authSettings.AccessTokenExpiryMinutes * 60));
    }
}
