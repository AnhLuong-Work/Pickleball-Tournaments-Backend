using AppPickleball.Application.Features.Tournaments.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Tournaments.Queries.GetTournaments;

public record GetTournamentsQuery(
    string? Search, string? Type, string? Status,
    int Page = 1, int PageSize = 20,
    string SortBy = "createdAt", string SortOrder = "desc"
) : IRequest<PagedResponse<TournamentDto>>;
