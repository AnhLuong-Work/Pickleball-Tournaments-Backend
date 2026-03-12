using AppPickleball.Application.Features.Users.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Users.Commands.UpdateProfile;

public record UpdateProfileCommand(
    string? Name, string? Bio, decimal? SkillLevel,
    string? DominantHand, string? PaddleType
) : IRequest<ApiResponse<UserProfileDto>>;
