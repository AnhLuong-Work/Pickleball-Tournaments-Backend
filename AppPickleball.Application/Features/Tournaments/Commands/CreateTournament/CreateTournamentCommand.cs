using AppPickleball.Application.Features.Tournaments.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Tournaments.Commands.CreateTournament;

public record CreateTournamentCommand(
    string Name, string? Description, string Type, int NumGroups,
    string? ScoringFormat, string? Date, string? Location
) : IRequest<ApiResponse<TournamentDetailDto>>;
