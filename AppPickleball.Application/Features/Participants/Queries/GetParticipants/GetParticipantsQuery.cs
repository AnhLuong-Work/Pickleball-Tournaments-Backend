using AppPickleball.Application.Features.Participants.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Participants.Queries.GetParticipants;

public record GetParticipantsQuery(Guid TournamentId, string? Status) : IRequest<ApiResponse<ParticipantListDto>>;
