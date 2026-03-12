using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Users.DTOs;
using AppPickleball.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Users.Queries.GetFollowers;

public class GetFollowersQueryHandler : IRequestHandler<GetFollowersQuery, ApiResponse<List<FollowUserDto>>>
{
    private readonly IBaseDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetFollowersQueryHandler(IBaseDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<List<FollowUserDto>>> Handle(GetFollowersQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        // Lấy danh sách user đang follow userId (followers)
        var items = await _db.Set<Follow>()
            .Where(f => f.FollowingId == userId)
            .Join(
                _db.Set<User>(),
                f => f.FollowerId,
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
