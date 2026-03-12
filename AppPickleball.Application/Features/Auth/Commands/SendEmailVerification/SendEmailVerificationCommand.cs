using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Auth.Commands.SendEmailVerification;

public record SendEmailVerificationCommand : IRequest<ApiResponse<SendVerificationResponseDto>>;
public record SendVerificationResponseDto(string Message, int ExpiresInSeconds);
