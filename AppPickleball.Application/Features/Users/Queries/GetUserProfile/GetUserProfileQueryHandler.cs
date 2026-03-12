using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Users.DTOs;
using AppPickleball.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Users.Queries.GetUserProfile;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, ApiResponse<PublicUserProfileDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IFollowRepository _followRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IBaseDbContext _db;

    public GetUserProfileQueryHandler(IUserRepository userRepo, IFollowRepository followRepo,
        ICurrentUserService currentUser, IBaseDbContext db)
    {
        _userRepo = userRepo; _followRepo = followRepo;
        _currentUser = currentUser; _db = db;
    }

    public async Task<ApiResponse<PublicUserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var target = await _userRepo.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("Người dùng không tồn tại");

        var currentUserId = _currentUser.UserId;
        var targetId = request.UserId;

        var totalTournaments = await _db.Set<AppPickleball.Domain.Entities.Participant>()
            .CountAsync(p => p.UserId == targetId && p.Status == ParticipantStatus.Confirmed, cancellationToken);

        var matches = await _db.Set<AppPickleball.Domain.Entities.Match>()
            .Where(m => m.Status == MatchStatus.Completed && (m.Player1Id == targetId || m.Player2Id == targetId))
            .ToListAsync(cancellationToken);

        var wins = matches.Count(m => m.WinnerId == targetId);
        var totalMatches = matches.Count;
        var winRate = totalMatches > 0 ? Math.Round((double)wins / totalMatches * 100, 1) : 0.0;

        var followingCount = await _db.Set<AppPickleball.Domain.Entities.Follow>()
            .CountAsync(f => f.FollowerId == targetId, cancellationToken);
        var followersCount = await _db.Set<AppPickleball.Domain.Entities.Follow>()
            .CountAsync(f => f.FollowingId == targetId, cancellationToken);

        var isFollowing = currentUserId != Guid.Empty && await _followRepo.IsFollowingAsync(currentUserId, targetId, cancellationToken);
        var isFollowedBy = currentUserId != Guid.Empty && await _followRepo.IsFollowingAsync(targetId, currentUserId, cancellationToken);

        // Head to head
        var h2hMatches = matches.Where(m => (m.Player1Id == currentUserId && m.Player2Id == targetId) ||
                                             (m.Player1Id == targetId && m.Player2Id == currentUserId)).ToList();
        HeadToHeadDto? headToHead = null;
        if (h2hMatches.Any())
        {
            var myWins = h2hMatches.Count(m => m.WinnerId == currentUserId);
            headToHead = new HeadToHeadDto(h2hMatches.Count, myWins, h2hMatches.Count - myWins, null);
        }

        var dto = new PublicUserProfileDto(
            target.Id, target.Name, target.AvatarUrl, target.Bio,
            target.SkillLevel, target.DominantHand, target.PaddleType,
            new UserStatsDto(totalTournaments, totalMatches, wins, totalMatches - wins, winRate, followingCount, followersCount),
            isFollowing, isFollowedBy, headToHead
        );

        return ApiResponse<PublicUserProfileDto>.SuccessResponse(dto);
    }
}
