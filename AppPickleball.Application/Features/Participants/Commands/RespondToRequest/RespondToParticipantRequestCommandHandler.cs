using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Participants.Interfaces;
using AppPickleball.Application.Features.Tournaments.Interfaces;
using AppPickleball.Application.Features.Participants.DTOs;
using AppPickleball.Domain.Enums;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Participants.Commands.RespondToRequest;

public class RespondToParticipantRequestCommandHandler : IRequestHandler<RespondToParticipantRequestCommand, ApiResponse<ParticipantDto>>
{
    private readonly ITournamentRepository _tournamentRepo;
    private readonly IParticipantRepository _participantRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public RespondToParticipantRequestCommandHandler(ITournamentRepository tournamentRepo, IParticipantRepository participantRepo,
        IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _tournamentRepo = tournamentRepo; _participantRepo = participantRepo;
        _uow = uow; _currentUser = currentUser;
    }

    public async Task<ApiResponse<ParticipantDto>> Handle(RespondToParticipantRequestCommand request, CancellationToken cancellationToken)
    {
        var tournament = await _tournamentRepo.GetByIdAsync(request.TournamentId, cancellationToken)
            ?? throw new NotFoundException("Giải đấu không tồn tại");

        if (tournament.CreatorId != _currentUser.UserId)
            throw new UnauthorizedException("Chỉ người tạo giải mới có thể duyệt");

        var participant = await _participantRepo.GetByIdAsync(request.ParticipantId, cancellationToken)
            ?? throw new NotFoundException("Yêu cầu không tồn tại");

        if (participant.Status != ParticipantStatus.RequestPending && participant.Status != ParticipantStatus.InvitedPending)
            throw new DomainException("Yêu cầu này đã được xử lý");

        if (request.Action == "approve")
        {
            var confirmedCount = await _participantRepo.CountConfirmedAsync(request.TournamentId, cancellationToken);
            if (confirmedCount >= tournament.MaxParticipants)
                throw new DomainException("Giải đã đủ người");

            participant.Status = ParticipantStatus.Confirmed;
            participant.JoinedAt = DateTime.UtcNow;
        }
        else if (request.Action == "reject")
        {
            participant.Status = ParticipantStatus.Rejected;
            participant.RejectReason = request.Reason;
        }
        else
        {
            throw new DomainException("Action phải là 'approve' hoặc 'reject'");
        }

        _participantRepo.Update(participant);
        await _uow.SaveChangesAsync(cancellationToken);

        var userDto = participant.User != null
            ? new UserBriefDto(participant.User.Id, participant.User.Name, participant.User.AvatarUrl, participant.User.SkillLevel)
            : new UserBriefDto(participant.UserId, "", null, 0);

        return ApiResponse<ParticipantDto>.SuccessResponse(
            new ParticipantDto(participant.Id, userDto, participant.Status.ToString(), participant.JoinedAt, participant.CreatedAt));
    }
}
