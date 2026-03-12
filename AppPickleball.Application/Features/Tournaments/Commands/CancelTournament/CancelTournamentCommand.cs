using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Tournaments.Commands.CancelTournament;

public record CancelTournamentCommand(Guid TournamentId, string? Reason) : IRequest<ApiResponse<object>>;
