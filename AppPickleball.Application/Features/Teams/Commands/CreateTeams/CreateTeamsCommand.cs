using AppPickleball.Application.Features.Teams.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Teams.Commands.CreateTeams;

public record TeamInput(string Name, Guid Player1Id, Guid Player2Id);

public record CreateTeamsCommand(Guid TournamentId, List<TeamInput> Teams)
    : IRequest<ApiResponse<List<TeamDto>>>;
