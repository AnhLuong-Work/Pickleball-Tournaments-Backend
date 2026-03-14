using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Participants.Interfaces;
using AppPickleball.Application.Features.Tournaments.Interfaces;
using AppPickleball.Domain.Entities;
using AppPickleball.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Tournaments.Commands.UpdateTournamentStatus;

public class UpdateTournamentStatusCommandHandler : IRequestHandler<UpdateTournamentStatusCommand, ApiResponse<object>>
{
    private readonly ITournamentRepository _tournamentRepo;
    private readonly IParticipantRepository _participantRepo;
    private readonly IBaseDbContext _db;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateTournamentStatusCommandHandler(ITournamentRepository tournamentRepo, IParticipantRepository participantRepo,
        IBaseDbContext db, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _tournamentRepo = tournamentRepo; _participantRepo = participantRepo;
        _db = db; _uow = uow; _currentUser = currentUser;
    }

    public async Task<ApiResponse<object>> Handle(UpdateTournamentStatusCommand request, CancellationToken cancellationToken)
    {
        var tournament = await _tournamentRepo.GetByIdAsync(request.TournamentId, cancellationToken)
            ?? throw new NotFoundException("Giải đấu không tồn tại");

        if (tournament.CreatorId != _currentUser.UserId)
            throw new UnauthorizedException("Không có quyền thay đổi trạng thái giải");

        if (!Enum.TryParse<TournamentStatus>(request.Status, true, out var targetStatus))
            throw new DomainException("Trạng thái không hợp lệ");

        // Validate transition
        ValidateTransition(tournament.Status, targetStatus);

        // Check preconditions for open→ready
        if (tournament.Status == TournamentStatus.Open && targetStatus == TournamentStatus.Ready)
        {
            var confirmedCount = await _participantRepo.CountConfirmedAsync(tournament.Id, cancellationToken);
            var needed = tournament.NumGroups * 4;
            if (confirmedCount < needed)
                throw new DomainException($"Cần ít nhất {needed} người xác nhận (hiện có {confirmedCount})");

            var groupCount = await _db.Set<Group>().CountAsync(g => g.TournamentId == tournament.Id, cancellationToken);
            if (groupCount < tournament.NumGroups)
                throw new DomainException("Chưa xếp bảng đấu");

            var matchCount = await _db.Set<Match>().CountAsync(m => m.TournamentId == tournament.Id, cancellationToken);
            if (matchCount == 0)
                throw new DomainException("Chưa tạo lịch thi đấu");

            if (tournament.Type == TournamentType.Doubles)
            {
                var teamCount = await _db.Set<Team>().CountAsync(t => t.TournamentId == tournament.Id, cancellationToken);
                if (teamCount == 0)
                    throw new DomainException("Chưa ghép đội");
            }
        }

        tournament.Status = targetStatus;
        _tournamentRepo.Update(tournament);
        await _uow.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.SuccessResponse(new { }, "Cập nhật trạng thái thành công");
    }

    private static void ValidateTransition(TournamentStatus current, TournamentStatus target)
    {
        var validTransitions = new Dictionary<TournamentStatus, TournamentStatus[]>
        {
            [TournamentStatus.Draft] = [TournamentStatus.Open, TournamentStatus.Cancelled],
            [TournamentStatus.Open] = [TournamentStatus.Ready, TournamentStatus.Cancelled],
            [TournamentStatus.Ready] = [TournamentStatus.InProgress, TournamentStatus.Cancelled],
            [TournamentStatus.InProgress] = [TournamentStatus.Completed],
        };

        if (!validTransitions.TryGetValue(current, out var allowed) || !allowed.Contains(target))
            throw new DomainException($"Không thể chuyển từ {current} sang {target}");
    }
}
