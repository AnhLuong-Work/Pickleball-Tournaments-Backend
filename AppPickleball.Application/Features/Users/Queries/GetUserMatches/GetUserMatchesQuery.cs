using AppPickleball.Application.Features.Users.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Users.Queries.GetUserMatches;

public record GetUserMatchesQuery(Guid UserId, int Page, int PageSize)
    : IRequest<ApiResponse<PagedResponse<UserMatchDto>>>;
