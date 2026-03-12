using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(string Otp) : IRequest<ApiResponse<VerifyEmailResponseDto>>;
public record VerifyEmailResponseDto(bool EmailVerified, DateTime EmailVerifiedAt);
