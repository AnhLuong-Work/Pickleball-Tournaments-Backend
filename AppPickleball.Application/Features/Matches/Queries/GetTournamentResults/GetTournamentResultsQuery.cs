using AppPickleball.Application.Features.Matches.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Matches.Queries.GetTournamentResults;

public record GetTournamentResultsQuery(Guid TournamentId) : IRequest<ApiResponse<TournamentResultDto>>;
