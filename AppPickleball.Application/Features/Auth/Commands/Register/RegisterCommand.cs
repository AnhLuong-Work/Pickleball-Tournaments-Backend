using AppPickleball.Application.Features.Auth.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password, string Name)
    : IRequest<ApiResponse<AuthResponseDto>>;
