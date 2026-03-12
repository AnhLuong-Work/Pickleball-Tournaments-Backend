using AppPickleball.Domain.Entities;

namespace AppPickleball.Application.Common.Services;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashToken(string token);
}
