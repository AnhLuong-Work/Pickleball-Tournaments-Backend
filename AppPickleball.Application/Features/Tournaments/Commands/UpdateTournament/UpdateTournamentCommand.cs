using AppPickleball.Application.Features.Tournaments.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Tournaments.Commands.UpdateTournament;

public record UpdateTournamentCommand(
    Guid TournamentId, string? Name, string? Description,
    string? Type, int? NumGroups, string? ScoringFormat,
    string? Date, string? Location
) : IRequest<ApiResponse<TournamentDetailDto>>;
