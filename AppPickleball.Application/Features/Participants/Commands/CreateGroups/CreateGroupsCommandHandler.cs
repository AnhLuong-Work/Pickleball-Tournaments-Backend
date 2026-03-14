using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Matches.Interfaces;
using AppPickleball.Application.Features.Participants.Interfaces;
using AppPickleball.Application.Features.Tournaments.Interfaces;
using AppPickleball.Domain.Entities;
using AppPickleball.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Shared.Kernel.Resources;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Participants.Commands.CreateGroups;

public class CreateGroupsCommandHandler : IRequestHandler<CreateGroupsCommand, ApiResponse<CreateGroupsResultDto>>
{
    private readonly ITournamentRepository _tournamentRepo;
    private readonly IGroupRepository _groupRepo;
    private readonly IMatchRepository _matchRepo;
    private readonly IParticipantRepository _participantRepo;
    private readonly IBaseDbContext _db;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CreateGroupsCommandHandler(ITournamentRepository tournamentRepo, IGroupRepository groupRepo,
        IMatchRepository matchRepo, IParticipantRepository participantRepo, IBaseDbContext db,
        IUnitOfWork uow, ICurrentUserService currentUser, IStringLocalizer<SharedResource> localizer)
    {
        _tournamentRepo = tournamentRepo; _groupRepo = groupRepo; _matchRepo = matchRepo;
        _participantRepo = participantRepo; _db = db; _uow = uow; _currentUser = currentUser;
        _localizer = localizer;
    }

    public async Task<ApiResponse<CreateGroupsResultDto>> Handle(CreateGroupsCommand request, CancellationToken cancellationToken)
    {
        var tournament = await _tournamentRepo.GetByIdAsync(request.TournamentId, cancellationToken)
            ?? throw new NotFoundException("Giải đấu không tồn tại");

        if (tournament.CreatorId != _currentUser.UserId)
            throw new UnauthorizedException(_localizer["Tournament_Creator_Only"]);

        if (request.Mode == "random")
        {
            // Preview mode — return random grouping without saving
            var confirmed = await _participantRepo.GetByTournamentAsync(request.TournamentId, ParticipantStatus.Confirmed, cancellationToken);
            var shuffled = confirmed.OrderBy(_ => Guid.NewGuid()).ToList();
            var previewGroups = new List<GroupResultDto>();
            var groupNames = new[] { "A", "B", "C", "D" };
            for (int i = 0; i < tournament.NumGroups; i++)
            {
                var members = shuffled.Skip(i * 4).Take(4).Select((p, idx) =>
                    new MemberResultDto(p.UserId, p.User?.Name ?? "", p.User?.AvatarUrl, p.User?.SkillLevel ?? 0, idx + 1)).ToList();
                previewGroups.Add(new GroupResultDto(Guid.NewGuid(), groupNames[i], members));
            }
            return ApiResponse<CreateGroupsResultDto>.SuccessResponse(
                new CreateGroupsResultDto(false, previewGroups, new List<MatchResultDto>(), 0));
        }

        // Manual mode — validate and save
        if (request.Groups == null || request.Groups.Count != tournament.NumGroups)
            throw new DomainException(_localizer["Groups_InvalidCount"]);

        foreach (var g in request.Groups)
            if (g.MemberIds.Count != 4)
                throw new DomainException(_localizer["Group_InvalidSize"]);

        var allMemberIds = request.Groups.SelectMany(g => g.MemberIds).ToList();
        if (allMemberIds.Distinct().Count() != allMemberIds.Count)
            throw new DomainException("Có thành viên thuộc nhiều bảng");

        // Delete existing groups and matches
        var existingGroups = await _groupRepo.GetByTournamentAsync(request.TournamentId, cancellationToken);
        foreach (var eg in existingGroups) _groupRepo.Remove(eg);

        var existingMatches = await _matchRepo.GetByTournamentAsync(request.TournamentId, cancellationToken);
        foreach (var em in existingMatches) _matchRepo.Remove(em);

        var createdGroups = new List<Group>();
        var createdMatches = new List<Match>();

        for (int gi = 0; gi < request.Groups.Count; gi++)
        {
            var groupInput = request.Groups[gi];
            var group = new Group
            {
                TournamentId = request.TournamentId,
                Name = groupInput.Name,
                DisplayOrder = gi + 1
            };
            await _groupRepo.AddAsync(group, cancellationToken);

            // Add members
            for (int mi = 0; mi < groupInput.MemberIds.Count; mi++)
            {
                var memberId = groupInput.MemberIds[mi];
                var member = new GroupMember
                {
                    GroupId = group.Id,
                    PlayerId = tournament.Type == TournamentType.Singles ? memberId : null,
                    TeamId = tournament.Type == TournamentType.Doubles ? memberId : null,
                    SeedOrder = mi + 1
                };
                _db.Set<GroupMember>().Add(member);
            }

            // Generate Round Robin matches: 3 rounds, 2 matches each
            // With 4 members (A=0, B=1, C=2, D=3):
            // Round 1: A-B (0-1), C-D (2-3)
            // Round 2: A-C (0-2), B-D (1-3)
            // Round 3: A-D (0-3), B-C (1-2)
            var roundPairings = new[]
            {
                new[] { (0, 1), (2, 3) },
                new[] { (0, 2), (1, 3) },
                new[] { (0, 3), (1, 2) }
            };

            for (int round = 0; round < 3; round++)
            {
                for (int matchOrd = 0; matchOrd < 2; matchOrd++)
                {
                    var (p1Idx, p2Idx) = roundPairings[round][matchOrd];
                    var match = new Match
                    {
                        TournamentId = request.TournamentId,
                        GroupId = group.Id,
                        Round = round + 1,
                        MatchOrder = matchOrd + 1,
                        Player1Id = groupInput.MemberIds[p1Idx],
                        Player2Id = groupInput.MemberIds[p2Idx],
                        Status = MatchStatus.Scheduled
                    };
                    await _matchRepo.AddAsync(match, cancellationToken);
                    createdMatches.Add(match);
                }
            }
            createdGroups.Add(group);
        }

        await _uow.SaveChangesAsync(cancellationToken);

        var groupDtos = createdGroups.Select(g => new GroupResultDto(g.Id, g.Name, new List<MemberResultDto>())).ToList();
        var matchDtos = createdMatches.Select(m => new MatchResultDto(
            m.Id, createdGroups.First(g => g.Id == m.GroupId).Name,
            m.Round, m.MatchOrder, m.Player1Id, m.Player2Id, m.Status.ToString().ToLower())).ToList();

        return ApiResponse<CreateGroupsResultDto>.SuccessResponse(
            new CreateGroupsResultDto(true, groupDtos, matchDtos, createdMatches.Count),
            _localizer["CreateGroups_Success"], 201);
    }
}
