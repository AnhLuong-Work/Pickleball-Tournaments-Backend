using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Users.Interfaces;
using AppPickleball.Application.Features.Users.DTOs;
using AppPickleball.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Users.Queries.GetMyProfile;

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, ApiResponse<UserProfileDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IBaseDbContext _db;

    public GetMyProfileQueryHandler(IUserRepository userRepo, ICurrentUserService currentUser, IBaseDbContext db)
    {
        _userRepo = userRepo; _currentUser = currentUser; _db = db;
    }

    public async Task<ApiResponse<UserProfileDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("User không tồn tại");

        var userId = _currentUser.UserId;

        // Stats query
        var totalTournaments = await _db.Set<AppPickleball.Domain.Entities.Participant>()
            .CountAsync(p => p.UserId == userId && p.Status == ParticipantStatus.Confirmed, cancellationToken);

        var matches = await _db.Set<AppPickleball.Domain.Entities.Match>()
            .Where(m => m.Status == MatchStatus.Completed && (m.Player1Id == userId || m.Player2Id == userId))
            .ToListAsync(cancellationToken);

        var wins = matches.Count(m => m.WinnerId == userId);
        var totalMatches = matches.Count;
        var losses = totalMatches - wins;
        var winRate = totalMatches > 0 ? Math.Round((double)wins / totalMatches * 100, 1) : 0.0;

        var followingCount = await _db.Set<AppPickleball.Domain.Entities.Follow>()
            .CountAsync(f => f.FollowerId == userId, cancellationToken);
        var followersCount = await _db.Set<AppPickleball.Domain.Entities.Follow>()
            .CountAsync(f => f.FollowingId == userId, cancellationToken);

        var dto = new UserProfileDto(
            user.Id, user.Email, user.Name, user.AvatarUrl, user.Bio,
            user.SkillLevel, user.DominantHand, user.PaddleType, user.EmailVerified,
            new UserStatsDto(totalTournaments, totalMatches, wins, losses, winRate, followingCount, followersCount),
            user.CreatedAt
        );

        return ApiResponse<UserProfileDto>.SuccessResponse(dto);
    }
}
