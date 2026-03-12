using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Features.Tournaments.DTOs;
using AppPickleball.Domain.Entities;
using AppPickleball.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Tournaments.Queries.GetTournaments;

public class GetTournamentsQueryHandler : IRequestHandler<GetTournamentsQuery, PagedResponse<TournamentDto>>
{
    private readonly IBaseDbContext _db;

    public GetTournamentsQueryHandler(IBaseDbContext db) => _db = db;

    public async Task<PagedResponse<TournamentDto>> Handle(GetTournamentsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Set<Tournament>()
            .Include(t => t.Creator)
            .Include(t => t.Participants)
            .Where(t => t.Status != TournamentStatus.Draft && t.Status != TournamentStatus.Cancelled)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(t => t.Name.ToLower().Contains(request.Search.ToLower()));

        if (!string.IsNullOrWhiteSpace(request.Type) && Enum.TryParse<TournamentType>(request.Type, true, out var typeEnum))
            query = query.Where(t => t.Type == typeEnum);

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<TournamentStatus>(request.Status, true, out var statusEnum))
            query = query.Where(t => t.Status == statusEnum);

        // Sort
        query = request.SortBy switch
        {
            "name" => request.SortOrder == "asc" ? query.OrderBy(t => t.Name) : query.OrderByDescending(t => t.Name),
            "date" => request.SortOrder == "asc" ? query.OrderBy(t => t.Date) : query.OrderByDescending(t => t.Date),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);

        var dtos = items.Select(t => MapToDto(t)).ToList();
        return PagedResponse<TournamentDto>.Create(dtos, request.Page, request.PageSize, totalCount);
    }

    private static TournamentDto MapToDto(Tournament t)
    {
        var count = t.Participants.Count(p => p.Status == ParticipantStatus.Confirmed);
        return new TournamentDto(
            t.Id, t.Name, t.Description, t.Type.ToString().ToLower(), t.NumGroups,
            t.ScoringFormat.ToString().ToLower().Replace("bestof", "best_of_"),
            t.Status.ToString().ToLower(), t.Date?.ToString("yyyy-MM-dd"),
            t.Location, t.BannerUrl,
            new CreatorDto(t.Creator.Id, t.Creator.Name, t.Creator.AvatarUrl),
            count, t.MaxParticipants, count >= t.MaxParticipants,
            t.CreatedAt, t.UpdatedAt
        );
    }
}
