using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Features.Matches.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Matches.Queries.GetMatches;

public class GetMatchesQueryHandler : IRequestHandler<GetMatchesQuery, ApiResponse<List<MatchDto>>>
{
    private readonly ITournamentRepository _tournamentRepo;
    private readonly IMatchRepository _matchRepo;
    private readonly IGroupRepository _groupRepo;

    public GetMatchesQueryHandler(ITournamentRepository tournamentRepo, IMatchRepository matchRepo, IGroupRepository groupRepo)
    {
        _tournamentRepo = tournamentRepo; _matchRepo = matchRepo; _groupRepo = groupRepo;
    }

    public async Task<ApiResponse<List<MatchDto>>> Handle(GetMatchesQuery request, CancellationToken cancellationToken)
    {
        var tournament = await _tournamentRepo.GetByIdAsync(request.TournamentId, cancellationToken)
            ?? throw new NotFoundException("Giải đấu không tồn tại");

        var matches = await _matchRepo.GetByTournamentAsync(request.TournamentId, cancellationToken);
        var groups = await _groupRepo.GetByTournamentAsync(request.TournamentId, cancellationToken);
        var groupMap = groups.ToDictionary(g => g.Id, g => g.Name);

        var dtos = matches.Select(m => new MatchDto(
            m.Id, groupMap.GetValueOrDefault(m.GroupId, ""), m.Round, m.MatchOrder,
            m.Player1Id, m.Player2Id, m.Player1Scores, m.Player2Scores,
            m.WinnerId, m.Status.ToString().ToLower()
        )).ToList();

        return ApiResponse<List<MatchDto>>.SuccessResponse(dtos);
    }
}
