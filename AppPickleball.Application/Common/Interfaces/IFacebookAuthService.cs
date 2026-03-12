namespace AppPickleball.Application.Common.Interfaces;

public record FacebookUserInfo(string FacebookId, string? Email, string? Name);

public interface IFacebookAuthService
{
    Task<FacebookUserInfo> VerifyAccessTokenAsync(string accessToken, CancellationToken ct = default);
}
