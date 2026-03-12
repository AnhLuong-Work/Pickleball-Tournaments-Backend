using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Features.Tournaments.DTOs;
using AppPickleball.Domain.Enums;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Tournaments.Queries.GetTournamentById;

public class GetTournamentByIdQueryHandler : IRequestHandler<GetTournamentByIdQuery, ApiResponse<TournamentDetailDto>>
{
    private readonly ITournamentRepository _tournamentRepo;
    private readonly IParticipantRepository _participantRepo;

    public GetTournamentByIdQueryHandler(ITournamentRepository tournamentRepo, IParticipantRepository participantRepo)
    {
        _tournamentRepo = tournamentRepo;
        _participantRepo = participantRepo;
    }

    public async Task<ApiResponse<TournamentDetailDto>> Handle(GetTournamentByIdQuery request, CancellationToken cancellationToken)
    {
        var tournament = await _tournamentRepo.GetWithDetailsAsync(request.TournamentId, cancellationToken)
            ?? throw new NotFoundException("Giải đấu không tồn tại");

        if (tournament.Status == TournamentStatus.Draft && tournament.CreatorId != request.CurrentUserId)
            throw new UnauthorizedException("Không có quyền xem giải này");

        var participants = await _participantRepo.GetByTournamentAsync(request.TournamentId, null, cancellationToken);
        var confirmedCount = participants.Count(p => p.Status == ParticipantStatus.Confirmed);

        // CurrentUser context
        CurrentUserTournamentDto? currentUserDto = null;
        if (request.CurrentUserId != Guid.Empty)
        {
            var myParticipant = participants.FirstOrDefault(p => p.UserId == request.CurrentUserId);
            if (request.CurrentUserId == tournament.CreatorId)
            {
                currentUserDto = new CurrentUserTournamentDto("creator", "creator", null, null);
            }
            else if (myParticipant != null)
            {
                var myGroup = tournament.Groups.FirstOrDefault(g => g.Members.Any(m => m.PlayerId == request.CurrentUserId ||
                    (m.TeamId.HasValue && (tournament.Teams.FirstOrDefault(t => t.Id == m.TeamId)?.Player1Id == request.CurrentUserId ||
                    tournament.Teams.FirstOrDefault(t => t.Id == m.TeamId)?.Player2Id == request.CurrentUserId))));
                currentUserDto = new CurrentUserTournamentDto("player", myParticipant.Status.ToString().ToLower(), myGroup?.Id, myGroup?.Name);
            }
        }

        // Map groups (only if status >= Ready)
        var groupDtos = new List<GroupDetailDto>();
        if (tournament.Status >= TournamentStatus.Ready)
        {
            foreach (var g in tournament.Groups.OrderBy(g => g.DisplayOrder))
            {
                var memberDtos = g.Members.OrderBy(m => m.SeedOrder).Select(m =>
                {
                    if (m.Player != null)
                        return new GroupMemberDto(m.Player.Id, m.Player.Name, m.Player.AvatarUrl, m.Player.SkillLevel, m.SeedOrder);
                    if (m.Team != null)
                        return new GroupMemberDto(m.Team.Id, m.Team.Name ?? "Đội", null, 0, m.SeedOrder);
                    return new GroupMemberDto(Guid.Empty, "Unknown", null, 0, m.SeedOrder);
                }).ToList();
                groupDtos.Add(new GroupDetailDto(g.Id, g.Name, memberDtos));
            }
        }

        var dto = new TournamentDetailDto(
            tournament.Id, tournament.Name, tournament.Description,
            tournament.Type.ToString().ToLower(), tournament.NumGroups,
            tournament.ScoringFormat.ToString().ToLower().Replace("bestof", "best_of_"),
            tournament.Status.ToString().ToLower(),
            tournament.Date?.ToString("yyyy-MM-dd"),
            tournament.Location, tournament.BannerUrl,
            new CreatorDto(tournament.Creator.Id, tournament.Creator.Name, tournament.Creator.AvatarUrl),
            confirmedCount, tournament.MaxParticipants,
            currentUserDto, groupDtos,
            tournament.CreatedAt, tournament.UpdatedAt
        );

        return ApiResponse<TournamentDetailDto>.SuccessResponse(dto);
    }
}
