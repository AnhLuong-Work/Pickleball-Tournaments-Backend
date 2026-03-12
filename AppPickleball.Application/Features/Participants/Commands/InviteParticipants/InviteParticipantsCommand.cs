using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Participants.Commands.InviteParticipants;

public record InviteParticipantsCommand(Guid TournamentId, List<Guid> UserIds) : IRequest<ApiResponse<InviteResultDto>>;
public record InviteResultDto(int Invited, int Skipped, List<InviteErrorDto> Errors);
public record InviteErrorDto(Guid UserId, string Reason);
