using AppPickleball.Application.Features.Auth.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<ApiResponse<AuthResponseDto>>;
