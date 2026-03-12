using AppPickleball.Application.Features.Participants.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Participants.Commands.RespondToRequest;

public record RespondToParticipantRequestCommand(
    Guid TournamentId, Guid ParticipantId, string Action, string? Reason
) : IRequest<ApiResponse<ParticipantDto>>;
