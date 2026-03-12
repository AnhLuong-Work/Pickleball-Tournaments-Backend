using AppPickleball.Application.Features.Tournaments.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Tournaments.Queries.GetTournamentById;

public record GetTournamentByIdQuery(Guid TournamentId, Guid CurrentUserId) : IRequest<ApiResponse<TournamentDetailDto>>;
