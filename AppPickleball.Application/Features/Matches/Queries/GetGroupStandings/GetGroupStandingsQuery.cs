using AppPickleball.Application.Features.Matches.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Matches.Queries.GetGroupStandings;

public record GetGroupStandingsQuery(Guid TournamentId, Guid GroupId) : IRequest<ApiResponse<List<StandingDto>>>;
