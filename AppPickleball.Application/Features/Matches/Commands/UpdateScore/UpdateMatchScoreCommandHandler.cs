using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Matches.DTOs;
using AppPickleball.Domain.Entities;
using AppPickleball.Domain.Enums;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Matches.Commands.UpdateScore;

public class UpdateMatchScoreCommandHandler : IRequestHandler<UpdateMatchScoreCommand, ApiResponse<MatchDto>>
{
    private readonly IMatchRepository _matchRepo;
    private readonly ITournamentRepository _tournamentRepo;
    private readonly IGroupRepository _groupRepo;
    private readonly IBaseDbContext _db;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateMatchScoreCommandHandler(IMatchRepository matchRepo, ITournamentRepository tournamentRepo,
        IGroupRepository groupRepo, IBaseDbContext db, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _matchRepo = matchRepo; _tournamentRepo = tournamentRepo; _groupRepo = groupRepo;
        _db = db; _uow = uow; _currentUser = currentUser;
    }

    public async Task<ApiResponse<MatchDto>> Handle(UpdateMatchScoreCommand request, CancellationToken cancellationToken)
    {
        var match = await _matchRepo.GetByIdAsync(request.MatchId, cancellationToken)
            ?? throw new NotFoundException("Trận đấu không tồn tại");

        var tournament = await _tournamentRepo.GetByIdAsync(match.TournamentId, cancellationToken)
            ?? throw new NotFoundException("Giải đấu không tồn tại");

        if (tournament.CreatorId != _currentUser.UserId)
            throw new UnauthorizedException("Chỉ người tạo giải mới có thể sửa điểm");

        if (match.Status != MatchStatus.Completed)
            throw new DomainException("Chỉ có thể sửa điểm của trận đã hoàn thành");

        if (request.Player1Scores.Length != request.Player2Scores.Length)
            throw new DomainException("Số set Player 1 và Player 2 phải bằng nhau");

        // Lưu lịch sử điểm trước khi cập nhật
        var history = new MatchScoreHistory
        {
            MatchId = match.Id,
            ModifiedBy = _currentUser.UserId,
            OldPlayer1Scores = match.Player1Scores,
            OldPlayer2Scores = match.Player2Scores,
            NewPlayer1Scores = request.Player1Scores,
            NewPlayer2Scores = request.Player2Scores,
            Reason = request.Reason
        };
        _db.Set<MatchScoreHistory>().Add(history);

        var p1Sets = request.Player1Scores.Zip(request.Player2Scores).Count(pair => pair.First > pair.Second);
        var winner = tournament.ScoringFormat == ScoringFormat.BestOf3
            ? (p1Sets >= 2 ? match.Player1Id : match.Player2Id)
            : (p1Sets >= 1 ? match.Player1Id : match.Player2Id);

        match.Player1Scores = request.Player1Scores;
        match.Player2Scores = request.Player2Scores;
        match.WinnerId = winner;
        _matchRepo.Update(match);

        await _uow.SaveChangesAsync(cancellationToken);

        var group = await _groupRepo.GetWithMembersAsync(match.GroupId, cancellationToken);
        var dto = new MatchDto(match.Id, group?.Name ?? "", match.Round, match.MatchOrder,
            match.Player1Id, match.Player2Id, match.Player1Scores, match.Player2Scores,
            match.WinnerId, match.Status.ToString().ToLower());

        return ApiResponse<MatchDto>.SuccessResponse(dto, "Cập nhật điểm thành công");
    }
}
