using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Features.Matches.DTOs;
using AppPickleball.Application.Features.Participants.Interfaces;
using AppPickleball.Application.Features.Tournaments.Interfaces;
using AppPickleball.Application.Features.Matches.Queries.GetGroupStandings;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Matches.Queries.GetTournamentResults;

public class GetTournamentResultsQueryHandler : IRequestHandler<GetTournamentResultsQuery, ApiResponse<TournamentResultDto>>
{
    private readonly ITournamentRepository _tournamentRepo;
    private readonly IGroupRepository _groupRepo;
    private readonly IMediator _mediator;

    public GetTournamentResultsQueryHandler(ITournamentRepository tournamentRepo, IGroupRepository groupRepo, IMediator mediator)
    {
        _tournamentRepo = tournamentRepo; _groupRepo = groupRepo; _mediator = mediator;
    }

    public async Task<ApiResponse<TournamentResultDto>> Handle(GetTournamentResultsQuery request, CancellationToken cancellationToken)
    {
        var tournament = await _tournamentRepo.GetByIdAsync(request.TournamentId, cancellationToken)
            ?? throw new NotFoundException("Giải đấu không tồn tại");

        var groups = await _groupRepo.GetByTournamentAsync(request.TournamentId, cancellationToken);
        var groupResults = new List<GroupResultStandingDto>();

        foreach (var group in groups.OrderBy(g => g.DisplayOrder))
        {
            var standingsResult = await _mediator.Send(new GetGroupStandingsQuery(request.TournamentId, group.Id), cancellationToken);
            groupResults.Add(new GroupResultStandingDto(group.Id, group.Name, standingsResult.Data ?? new List<StandingDto>()));
        }

        var result = new TournamentResultDto(
            tournament.Id, tournament.Name, tournament.Status.ToString().ToLower(), groupResults);

        return ApiResponse<TournamentResultDto>.SuccessResponse(result);
    }
}
