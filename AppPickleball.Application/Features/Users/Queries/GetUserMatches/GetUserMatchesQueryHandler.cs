using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Features.Users.DTOs;
using AppPickleball.Domain.Entities;
using AppPickleball.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Users.Queries.GetUserMatches;

public class GetUserMatchesQueryHandler : IRequestHandler<GetUserMatchesQuery, ApiResponse<PagedResponse<UserMatchDto>>>
{
    private readonly IBaseDbContext _db;

    public GetUserMatchesQueryHandler(IBaseDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResponse<PagedResponse<UserMatchDto>>> Handle(GetUserMatchesQuery request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;

        // Base query: các trận hoàn thành mà user tham gia
        var matchQuery = _db.Set<Match>()
            .Where(m => m.Status == MatchStatus.Completed &&
                        (m.Player1Id == userId || m.Player2Id == userId));

        var total = await matchQuery.CountAsync(cancellationToken);

        // Join với Group và Tournament để lấy tên
        var rawItems = await matchQuery
            .OrderByDescending(m => m.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Join(
                _db.Set<Group>(),
                m => m.GroupId,
                g => g.Id,
                (m, g) => new { m, g }
            )
            .Join(
                _db.Set<Tournament>(),
                x => x.m.TournamentId,
                t => t.Id,
                (x, t) => new { x.m, x.g, t }
            )
            .ToListAsync(cancellationToken);

        // Lấy danh sách opponent Ids để query User info
        var opponentIds = rawItems
            .Select(x => x.m.Player1Id == userId ? x.m.Player2Id : x.m.Player1Id)
            .Distinct()
            .ToList();

        var opponents = await _db.Set<User>()
            .Where(u => opponentIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        var items = rawItems.Select(x =>
        {
            var isPlayer1 = x.m.Player1Id == userId;
            var opponentId = isPlayer1 ? x.m.Player2Id : x.m.Player1Id;
            var myScores = isPlayer1 ? x.m.Player1Scores : x.m.Player2Scores;
            var opponentScores = isPlayer1 ? x.m.Player2Scores : x.m.Player1Scores;
            var won = x.m.WinnerId == userId;

            opponents.TryGetValue(opponentId, out var opponent);

            return new UserMatchDto(
                x.m.Id,
                x.t.Id,
                x.t.Name,
                x.g.Name,
                x.m.Round,
                x.m.MatchOrder,
                opponentId,
                opponent?.Name ?? "Unknown",
                opponent?.AvatarUrl,
                myScores,
                opponentScores,
                won,
                x.m.CreatedAt
            );
        }).ToList();

        var paged = PagedResponse<UserMatchDto>.Create(items, request.Page, request.PageSize, total);
        return ApiResponse<PagedResponse<UserMatchDto>>.SuccessResponse(paged);
    }
}
