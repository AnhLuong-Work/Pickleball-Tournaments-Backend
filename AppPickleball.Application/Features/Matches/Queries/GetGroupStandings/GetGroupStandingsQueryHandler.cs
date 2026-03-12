using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Features.Matches.DTOs;
using AppPickleball.Domain.Enums;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Matches.Queries.GetGroupStandings;

public class GetGroupStandingsQueryHandler : IRequestHandler<GetGroupStandingsQuery, ApiResponse<List<StandingDto>>>
{
    private readonly IGroupRepository _groupRepo;
    private readonly IMatchRepository _matchRepo;

    public GetGroupStandingsQueryHandler(IGroupRepository groupRepo, IMatchRepository matchRepo)
    {
        _groupRepo = groupRepo; _matchRepo = matchRepo;
    }

    public async Task<ApiResponse<List<StandingDto>>> Handle(GetGroupStandingsQuery request, CancellationToken cancellationToken)
    {
        var group = await _groupRepo.GetWithMembersAsync(request.GroupId, cancellationToken)
            ?? throw new NotFoundException("Bảng đấu không tồn tại");

        if (group.TournamentId != request.TournamentId)
            throw new DomainException("Bảng không thuộc giải này");

        var matches = await _matchRepo.GetByGroupAsync(request.GroupId, cancellationToken);
        var completedMatches = matches.Where(m => m.Status == MatchStatus.Completed || m.Status == MatchStatus.Walkover).ToList();

        // Lấy tất cả player/team IDs từ group members
        var playerIds = group.Members
            .Select(m => m.PlayerId ?? m.TeamId ?? Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToList();

        var standings = playerIds.Select(playerId =>
        {
            var member = group.Members.FirstOrDefault(m => m.PlayerId == playerId || m.TeamId == playerId);
            var playerMatches = completedMatches.Where(m => m.Player1Id == playerId || m.Player2Id == playerId).ToList();
            var wins = playerMatches.Count(m => m.WinnerId == playerId);
            var losses = playerMatches.Count - wins;

            int setsWon = 0, setsLost = 0;
            foreach (var m in playerMatches)
            {
                if (m.Player1Id == playerId)
                {
                    setsWon += m.Player1Scores?.Count(s => s > 0) ?? 0;
                    setsLost += m.Player2Scores?.Count(s => s > 0) ?? 0;
                }
                else
                {
                    setsWon += m.Player2Scores?.Count(s => s > 0) ?? 0;
                    setsLost += m.Player1Scores?.Count(s => s > 0) ?? 0;
                }
            }

            string name = member?.Player?.Name ?? member?.Team?.Name ?? "Unknown";
            string? avatarUrl = member?.Player?.AvatarUrl;

            return new StandingDto(playerId, name, avatarUrl, playerMatches.Count, wins, losses, setsWon, setsLost, wins * 3);
        })
        .OrderByDescending(s => s.Points).ThenByDescending(s => s.SetsWon - s.SetsLost)
        .ToList();

        return ApiResponse<List<StandingDto>>.SuccessResponse(standings);
    }
}
