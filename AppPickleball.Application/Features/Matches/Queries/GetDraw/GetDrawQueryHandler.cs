using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Features.Matches.DTOs;
using AppPickleball.Domain.Entities;
using AppPickleball.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Matches.Queries.GetDraw;

public class GetDrawQueryHandler : IRequestHandler<GetDrawQuery, ApiResponse<DrawDto>>
{
    private readonly ITournamentRepository _tournamentRepo;
    private readonly IGroupRepository _groupRepo;
    private readonly IMatchRepository _matchRepo;
    private readonly ITeamRepository _teamRepo;
    private readonly IBaseDbContext _db;

    public GetDrawQueryHandler(
        ITournamentRepository tournamentRepo,
        IGroupRepository groupRepo,
        IMatchRepository matchRepo,
        ITeamRepository teamRepo,
        IBaseDbContext db)
    {
        _tournamentRepo = tournamentRepo;
        _groupRepo = groupRepo;
        _matchRepo = matchRepo;
        _teamRepo = teamRepo;
        _db = db;
    }

    public async Task<ApiResponse<DrawDto>> Handle(GetDrawQuery request, CancellationToken cancellationToken)
    {
        // 1. Load tournament, check exists
        var tournament = await _tournamentRepo.GetByIdAsync(request.TournamentId, cancellationToken)
            ?? throw new NotFoundException("Giải đấu không tồn tại");

        // 2. Nếu chưa Ready trả về empty groups
        if (tournament.Status == TournamentStatus.Draft || tournament.Status == TournamentStatus.Open)
        {
            var emptyDraw = new DrawDto(
                tournament.Id,
                tournament.Name,
                tournament.Type.ToString(),
                tournament.Status.ToString(),
                new List<DrawGroupDto>()
            );
            return ApiResponse<DrawDto>.SuccessResponse(emptyDraw);
        }

        // 3. Load all groups for tournament
        var groups = await _groupRepo.GetByTournamentAsync(request.TournamentId, cancellationToken);

        // 4. Load group members with nav props, load matches
        var groupsWithMembers = new List<Group>();
        foreach (var group in groups)
        {
            var groupWithMembers = await _groupRepo.GetWithMembersAsync(group.Id, cancellationToken);
            if (groupWithMembers != null)
                groupsWithMembers.Add(groupWithMembers);
        }

        var allMatches = await _matchRepo.GetByTournamentAsync(request.TournamentId, cancellationToken);

        // 5. Load user info
        var allPlayerIds = allMatches.SelectMany(m => new[] { m.Player1Id, m.Player2Id }).Distinct().ToList();
        var memberPlayerIds = groupsWithMembers
            .SelectMany(g => g.Members)
            .Where(m => m.PlayerId.HasValue)
            .Select(m => m.PlayerId!.Value)
            .Distinct()
            .ToList();
        var allUserIds = allPlayerIds.Concat(memberPlayerIds).Distinct().ToList();

        var users = await _db.Set<User>()
            .Where(u => allUserIds.Contains(u.Id))
            .ToListAsync(cancellationToken);
        var userMap = users.ToDictionary(u => u.Id);

        // 6. Load team info cho Doubles
        Dictionary<Guid, Team> teamMap = new();
        if (tournament.Type == TournamentType.Doubles)
        {
            var teams = await _teamRepo.GetByTournamentAsync(request.TournamentId, cancellationToken);
            teamMap = teams.ToDictionary(t => t.Id);
        }

        // Helper: lấy display name và avatar từ playerId (có thể là UserId hoặc TeamId)
        string GetName(Guid id) =>
            tournament.Type == TournamentType.Doubles
                ? (teamMap.TryGetValue(id, out var t) ? t.Name ?? "Unknown" : "Unknown")
                : (userMap.TryGetValue(id, out var u) ? u.Name : "Unknown");

        string? GetAvatar(Guid id) =>
            tournament.Type == TournamentType.Doubles
                ? null
                : (userMap.TryGetValue(id, out var u) ? u.AvatarUrl : null);

        // 7. Map sang DrawDto
        var drawGroups = groupsWithMembers.OrderBy(g => g.DisplayOrder).Select(group =>
        {
            var groupMatches = allMatches
                .Where(m => m.GroupId == group.Id)
                .OrderBy(m => m.Round).ThenBy(m => m.MatchOrder)
                .ToList();

            var members = group.Members.OrderBy(m => m.SeedOrder).Select(member =>
            {
                var memberId = member.PlayerId ?? member.TeamId ?? Guid.Empty;
                string name;
                string? avatarUrl = null;

                if (tournament.Type == TournamentType.Doubles && member.TeamId.HasValue)
                {
                    name = teamMap.TryGetValue(member.TeamId.Value, out var team)
                        ? team.Name ?? "Unknown"
                        : "Unknown";
                }
                else if (member.Player != null)
                {
                    name = member.Player.Name;
                    avatarUrl = member.Player.AvatarUrl;
                }
                else if (member.PlayerId.HasValue && userMap.TryGetValue(member.PlayerId.Value, out var user))
                {
                    name = user.Name;
                    avatarUrl = user.AvatarUrl;
                }
                else
                {
                    name = "Unknown";
                }

                return new DrawMemberDto(memberId, name, avatarUrl, member.SeedOrder);
            }).ToList();

            var matchDtos = groupMatches.Select(m => new DrawMatchDto(
                m.Id,
                m.Round,
                m.MatchOrder,
                m.Player1Id,
                GetName(m.Player1Id),
                GetAvatar(m.Player1Id),
                m.Player2Id,
                GetName(m.Player2Id),
                GetAvatar(m.Player2Id),
                m.Player1Scores,
                m.Player2Scores,
                m.WinnerId,
                m.Status.ToString()
            )).ToList();

            return new DrawGroupDto(group.Id, group.Name, group.DisplayOrder, members, matchDtos);
        }).ToList();

        var drawDto = new DrawDto(
            tournament.Id,
            tournament.Name,
            tournament.Type.ToString(),
            tournament.Status.ToString(),
            drawGroups
        );

        return ApiResponse<DrawDto>.SuccessResponse(drawDto);
    }
}
