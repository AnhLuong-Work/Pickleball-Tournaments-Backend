using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Features.Participants.DTOs;
using AppPickleball.Domain.Enums;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Participants.Queries.GetParticipants;

public class GetParticipantsQueryHandler : IRequestHandler<GetParticipantsQuery, ApiResponse<ParticipantListDto>>
{
    private readonly ITournamentRepository _tournamentRepo;
    private readonly IParticipantRepository _participantRepo;

    public GetParticipantsQueryHandler(ITournamentRepository tournamentRepo, IParticipantRepository participantRepo)
    {
        _tournamentRepo = tournamentRepo; _participantRepo = participantRepo;
    }

    public async Task<ApiResponse<ParticipantListDto>> Handle(GetParticipantsQuery request, CancellationToken cancellationToken)
    {
        var tournament = await _tournamentRepo.GetByIdAsync(request.TournamentId, cancellationToken)
            ?? throw new NotFoundException("Giải đấu không tồn tại");

        ParticipantStatus? statusFilter = null;
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<ParticipantStatus>(request.Status, true, out var s))
            statusFilter = s;

        var participants = await _participantRepo.GetByTournamentAsync(request.TournamentId, statusFilter, cancellationToken);

        var dtos = participants.Select(p => new ParticipantDto(
            p.Id,
            p.User != null ? new UserBriefDto(p.User.Id, p.User.Name, p.User.AvatarUrl, p.User.SkillLevel)
                           : new UserBriefDto(p.UserId, "", null, 0),
            p.Status.ToString(), p.JoinedAt, p.CreatedAt
        )).ToList();

        var allParticipants = await _participantRepo.GetByTournamentAsync(request.TournamentId, null, cancellationToken);
        var result = new ParticipantListDto(
            dtos,
            allParticipants.Count(p => p.Status == ParticipantStatus.Confirmed),
            allParticipants.Count(p => p.Status == ParticipantStatus.InvitedPending),
            allParticipants.Count(p => p.Status == ParticipantStatus.RequestPending),
            tournament.MaxParticipants
        );

        return ApiResponse<ParticipantListDto>.SuccessResponse(result);
    }
}
