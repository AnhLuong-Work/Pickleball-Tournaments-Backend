using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Domain.Enums;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Tournaments.Commands.CancelTournament;

public class CancelTournamentCommandHandler : IRequestHandler<CancelTournamentCommand, ApiResponse<object>>
{
    private readonly ITournamentRepository _tournamentRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CancelTournamentCommandHandler(ITournamentRepository tournamentRepo, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _tournamentRepo = tournamentRepo; _uow = uow; _currentUser = currentUser;
    }

    public async Task<ApiResponse<object>> Handle(CancelTournamentCommand request, CancellationToken cancellationToken)
    {
        var tournament = await _tournamentRepo.GetByIdAsync(request.TournamentId, cancellationToken)
            ?? throw new NotFoundException("Giải đấu không tồn tại");

        if (tournament.CreatorId != _currentUser.UserId)
            throw new UnauthorizedException("Không có quyền hủy giải này");

        if (tournament.Status == TournamentStatus.Completed)
            throw new DomainException("Không thể hủy giải đã kết thúc");

        if (tournament.Status == TournamentStatus.InProgress && string.IsNullOrWhiteSpace(request.Reason))
            throw new DomainException("Phải nhập lý do khi hủy giải đang thi đấu");

        tournament.Status = TournamentStatus.Cancelled;
        _tournamentRepo.Update(tournament);
        await _uow.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.SuccessResponse(new { }, "Đã hủy giải đấu", 204);
    }
}
