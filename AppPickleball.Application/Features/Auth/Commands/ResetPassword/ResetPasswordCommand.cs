using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(string Email, string Otp, string NewPassword) : IRequest<ApiResponse<ResetPasswordResponseDto>>;
public record ResetPasswordResponseDto(string Message);
