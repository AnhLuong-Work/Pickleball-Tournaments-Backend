using System.Text.Json;
using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;

namespace AppPickleball.Infrastructure.Services;

public class FacebookAuthService : IFacebookAuthService
{
    private readonly HttpClient _httpClient;

    public FacebookAuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<FacebookUserInfo> VerifyAccessTokenAsync(string accessToken, CancellationToken ct = default)
    {
        var url = $"https://graph.facebook.com/me?fields=id,name,email&access_token={accessToken}";

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(url, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new UnauthorizedException($"Không thể kết nối Facebook Graph API: {ex.Message}");
        }

        var content = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new UnauthorizedException("Facebook access token không hợp lệ hoặc đã hết hạn");

        var json = JsonDocument.Parse(content).RootElement;

        var facebookId = json.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        if (string.IsNullOrEmpty(facebookId))
            throw new UnauthorizedException("Facebook access token không hợp lệ");

        var email = json.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
        var name = json.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;

        return new FacebookUserInfo(facebookId, email, name);
    }
}
