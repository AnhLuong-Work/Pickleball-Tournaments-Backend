using AppPickleball.Application.Features.Users.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Users.Queries.GetMyTournaments;

public record GetMyTournamentsQuery(string? Status, int Page, int PageSize)
    : IRequest<ApiResponse<PagedResponse<UserTournamentDto>>>;
