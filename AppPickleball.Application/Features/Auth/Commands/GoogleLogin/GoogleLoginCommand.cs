using AppPickleball.Application.Features.Auth.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Auth.Commands.GoogleLogin;

public record GoogleLoginCommand(string IdToken) : IRequest<ApiResponse<AuthResponseDto>>;
