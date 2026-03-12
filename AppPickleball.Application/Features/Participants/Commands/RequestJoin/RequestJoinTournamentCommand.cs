using AppPickleball.Application.Features.Participants.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Participants.Commands.RequestJoin;

public record RequestJoinTournamentCommand(Guid TournamentId) : IRequest<ApiResponse<ParticipantDto>>;
