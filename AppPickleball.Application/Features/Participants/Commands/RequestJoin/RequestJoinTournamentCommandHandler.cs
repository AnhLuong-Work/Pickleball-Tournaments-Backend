using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Participants.Interfaces;
using AppPickleball.Application.Features.Tournaments.Interfaces;
using AppPickleball.Application.Features.Participants.DTOs;
using AppPickleball.Domain.Entities;
using AppPickleball.Domain.Enums;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Participants.Commands.RequestJoin;

public class RequestJoinTournamentCommandHandler : IRequestHandler<RequestJoinTournamentCommand, ApiResponse<ParticipantDto>>
{
    private readonly ITournamentRepository _tournamentRepo;
    private readonly IParticipantRepository _participantRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public RequestJoinTournamentCommandHandler(ITournamentRepository tournamentRepo, IParticipantRepository participantRepo,
        IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _tournamentRepo = tournamentRepo; _participantRepo = participantRepo;
        _uow = uow; _currentUser = currentUser;
    }

    public async Task<ApiResponse<ParticipantDto>> Handle(RequestJoinTournamentCommand request, CancellationToken cancellationToken)
    {
        var tournament = await _tournamentRepo.GetByIdAsync(request.TournamentId, cancellationToken)
            ?? throw new NotFoundException("Giải đấu không tồn tại");

        if (tournament.Status != TournamentStatus.Open)
            throw new DomainException("Giải không trong trạng thái mở đăng ký");

        var existing = await _participantRepo.GetByTournamentAndUserAsync(request.TournamentId, _currentUser.UserId, cancellationToken);
        if (existing != null)
        {
            if (existing.Status == ParticipantStatus.Confirmed)
                throw new DomainException("Bạn đã ở trong giải này rồi");
            if (existing.Status == ParticipantStatus.RequestPending || existing.Status == ParticipantStatus.InvitedPending)
                throw new DomainException("Đã có yêu cầu đang chờ xử lý");
        }

        var confirmedCount = await _participantRepo.CountConfirmedAsync(request.TournamentId, cancellationToken);
        if (confirmedCount >= tournament.MaxParticipants)
            throw new DomainException("Giải đã đủ người tham gia");

        var participant = new Participant
        {
            TournamentId = request.TournamentId,
            UserId = _currentUser.UserId,
            Status = ParticipantStatus.RequestPending
        };
        await _participantRepo.AddAsync(participant, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return ApiResponse<ParticipantDto>.SuccessResponse(
            new ParticipantDto(participant.Id, new UserBriefDto(Guid.Empty, "", null, 0),
                participant.Status.ToString(), null, participant.CreatedAt),
            "Yêu cầu tham gia đã được gửi", 201);
    }
}
