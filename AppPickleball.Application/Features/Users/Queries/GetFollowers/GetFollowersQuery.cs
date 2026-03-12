using AppPickleball.Application.Features.Users.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Users.Queries.GetFollowers;

public record GetFollowersQuery : IRequest<ApiResponse<List<FollowUserDto>>>;
