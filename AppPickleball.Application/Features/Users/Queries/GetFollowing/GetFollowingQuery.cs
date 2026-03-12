using AppPickleball.Application.Features.Users.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Users.Queries.GetFollowing;

public record GetFollowingQuery : IRequest<ApiResponse<List<FollowUserDto>>>;
