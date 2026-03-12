using AppPickleball.Application.Features.Users.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Users.Queries.GetMyProfile;

public record GetMyProfileQuery : IRequest<ApiResponse<UserProfileDto>>;
