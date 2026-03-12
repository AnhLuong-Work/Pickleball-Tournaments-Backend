namespace AppPickleball.Application.Features.Auth.DTOs;

public record TokenResponseDto(string AccessToken, string RefreshToken, int ExpiresIn);
