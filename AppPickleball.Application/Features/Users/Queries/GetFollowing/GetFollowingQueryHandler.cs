using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Users.DTOs;
using AppPickleball.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Users.Queries.GetFollowing;

public class GetFollowingQueryHandler : IRequestHandler<GetFollowingQuery, ApiResponse<List<FollowUserDto>>>
{
    private readonly IBaseDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetFollowingQueryHandler(IBaseDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<List<FollowUserDto>>> Handle(GetFollowingQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        // Lấy danh sách user mà userId đang follow
        var items = await _db.Set<Follow>()
            .Where(f => f.FollowerId == userId)
            .Join(
                _db.Set<User>(),
                f => f.FollowingId,
                u => u.Id,
                (f, u) => new FollowUserDto(
                    u.Id,
                    u.Name,
                    u.AvatarUrl,
                    u.SkillLevel,
                    f.CreatedAt
                )
            )
            .ToListAsync(cancellationToken);

        return ApiResponse<List<FollowUserDto>>.SuccessResponse(items);
    }
}
