using AppPickleball.Application.Features.Auth.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Auth.Commands.FacebookLogin;

public record FacebookLoginCommand(string AccessToken) : IRequest<ApiResponse<AuthResponseDto>>;
