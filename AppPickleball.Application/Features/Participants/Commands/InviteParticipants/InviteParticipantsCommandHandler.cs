using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Participants.Interfaces;
using AppPickleball.Application.Features.Tournaments.Interfaces;
using AppPickleball.Application.Features.Users.Interfaces;
using AppPickleball.Domain.Entities;
using AppPickleball.Domain.Enums;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Participants.Commands.InviteParticipants;

public class InviteParticipantsCommandHandler : IRequestHandler<InviteParticipantsCommand, ApiResponse<InviteResultDto>>
{
    private readonly ITournamentRepository _tournamentRepo;
    private readonly IParticipantRepository _participantRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public InviteParticipantsCommandHandler(ITournamentRepository tournamentRepo, IParticipantRepository participantRepo,
        IUserRepository userRepo, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _tournamentRepo = tournamentRepo; _participantRepo = participantRepo;
        _userRepo = userRepo; _uow = uow; _currentUser = currentUser;
    }

    public async Task<ApiResponse<InviteResultDto>> Handle(InviteParticipantsCommand request, CancellationToken cancellationToken)
    {
        var tournament = await _tournamentRepo.GetByIdAsync(request.TournamentId, cancellationToken)
            ?? throw new NotFoundException("Giải đấu không tồn tại");

        if (tournament.CreatorId != _currentUser.UserId)
            throw new UnauthorizedException("Chỉ người tạo giải mới có thể mời");

        if (tournament.Status != TournamentStatus.Open)
            throw new DomainException("Giải không trong trạng thái mở đăng ký");

        var errors = new List<InviteErrorDto>();
        var invited = 0;
        var skipped = 0;

        foreach (var userId in request.UserIds.Take(20))
        {
            var user = await _userRepo.GetByIdAsync(userId, cancellationToken);
            if (user == null) { errors.Add(new InviteErrorDto(userId, "USER_NOT_FOUND")); skipped++; continue; }

            var existing = await _participantRepo.GetByTournamentAndUserAsync(request.TournamentId, userId, cancellationToken);
            if (existing != null) { errors.Add(new InviteErrorDto(userId, "ALREADY_IN_TOURNAMENT")); skipped++; continue; }

            var confirmedCount = await _participantRepo.CountConfirmedAsync(request.TournamentId, cancellationToken);
            if (confirmedCount >= tournament.MaxParticipants) { errors.Add(new InviteErrorDto(userId, "TOURNAMENT_FULL")); skipped++; continue; }

            await _participantRepo.AddAsync(new Participant
            {
                TournamentId = request.TournamentId, UserId = userId,
                Status = ParticipantStatus.InvitedPending, InvitedBy = _currentUser.UserId
            }, cancellationToken);
            invited++;
        }

        await _uow.SaveChangesAsync(cancellationToken);
        return ApiResponse<InviteResultDto>.SuccessResponse(new InviteResultDto(invited, skipped, errors));
    }
}
