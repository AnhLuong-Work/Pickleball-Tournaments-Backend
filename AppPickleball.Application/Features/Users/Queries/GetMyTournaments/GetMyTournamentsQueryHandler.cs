using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Users.DTOs;
using AppPickleball.Domain.Entities;
using AppPickleball.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Users.Queries.GetMyTournaments;

public class GetMyTournamentsQueryHandler : IRequestHandler<GetMyTournamentsQuery, ApiResponse<PagedResponse<UserTournamentDto>>>
{
    private readonly IBaseDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyTournamentsQueryHandler(IBaseDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResponse<UserTournamentDto>>> Handle(GetMyTournamentsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        // Base query: participant đã confirmed
        var query = _db.Set<Participant>()
            .Where(p => p.UserId == userId && p.Status == ParticipantStatus.Confirmed)
            .Join(
                _db.Set<Tournament>(),
                p => p.TournamentId,
                t => t.Id,
                (p, t) => new { p, t }
            );

        // Optional filter theo tournament status
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<TournamentStatus>(request.Status, ignoreCase: true, out var statusEnum))
        {
            query = query.Where(x => x.t.Status == statusEnum);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.t.Date)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new UserTournamentDto(
                x.t.Id,
                x.t.Name,
                x.t.Type.ToString(),
                x.t.Status.ToString(),
                x.t.Date.HasValue ? x.t.Date.Value.ToString("yyyy-MM-dd") : null,
                x.t.Location,
                x.t.BannerUrl,
                x.t.CreatorId == userId ? "Creator" : "Player",
                x.p.JoinedAt
            ))
            .ToListAsync(cancellationToken);

        var paged = PagedResponse<UserTournamentDto>.Create(items, request.Page, request.PageSize, total);
        return ApiResponse<PagedResponse<UserTournamentDto>>.SuccessResponse(paged);
    }
}
