using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Settings;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace AppPickleball.Infrastructure.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly GoogleAuthSettings _settings;

    public GoogleAuthService(IOptions<GoogleAuthSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<GoogleUserInfo> VerifyIdTokenAsync(string idToken, CancellationToken ct = default)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _settings.ClientId }
                });

            return new GoogleUserInfo(
                GoogleId: payload.Subject,
                Email: payload.Email,
                Name: payload.Name,
                Picture: payload.Picture);
        }
        catch (InvalidJwtException ex)
        {
            throw new UnauthorizedException($"Google ID Token không hợp lệ: {ex.Message}");
        }
    }
}
