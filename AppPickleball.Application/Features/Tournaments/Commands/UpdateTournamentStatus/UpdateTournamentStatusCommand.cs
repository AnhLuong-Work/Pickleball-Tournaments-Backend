using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Tournaments.Commands.UpdateTournamentStatus;

public record UpdateTournamentStatusCommand(Guid TournamentId, string Status) : IRequest<ApiResponse<object>>;
