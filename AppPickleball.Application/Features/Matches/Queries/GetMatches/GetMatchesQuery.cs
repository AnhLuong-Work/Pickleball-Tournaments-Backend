using AppPickleball.Application.Features.Matches.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Matches.Queries.GetMatches;

public record GetMatchesQuery(Guid TournamentId) : IRequest<ApiResponse<List<MatchDto>>>;
