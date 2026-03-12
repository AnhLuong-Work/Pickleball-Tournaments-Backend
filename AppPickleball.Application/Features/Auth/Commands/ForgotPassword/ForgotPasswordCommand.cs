using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<ApiResponse<ForgotPasswordResponseDto>>;
public record ForgotPasswordResponseDto(string Message, int ExpiresInSeconds);
