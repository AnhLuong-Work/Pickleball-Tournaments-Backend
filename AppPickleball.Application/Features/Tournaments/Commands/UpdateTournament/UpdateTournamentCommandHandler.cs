using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Tournaments.DTOs;
using AppPickleball.Application.Features.Tournaments.Queries.GetTournamentById;
using AppPickleball.Domain.Enums;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Tournaments.Commands.UpdateTournament;

public class UpdateTournamentCommandHandler : IRequestHandler<UpdateTournamentCommand, ApiResponse<TournamentDetailDto>>
{
    private readonly ITournamentRepository _tournamentRepo;
    private readonly IParticipantRepository _participantRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IMediator _mediator;

    public UpdateTournamentCommandHandler(ITournamentRepository tournamentRepo, IParticipantRepository participantRepo,
        IUnitOfWork uow, ICurrentUserService currentUser, IMediator mediator)
    {
        _tournamentRepo = tournamentRepo; _participantRepo = participantRepo;
        _uow = uow; _currentUser = currentUser; _mediator = mediator;
    }

    public async Task<ApiResponse<TournamentDetailDto>> Handle(UpdateTournamentCommand request, CancellationToken cancellationToken)
    {
        var tournament = await _tournamentRepo.GetByIdAsync(request.TournamentId, cancellationToken)
            ?? throw new NotFoundException("Giải đấu không tồn tại");

        if (tournament.CreatorId != _currentUser.UserId)
            throw new UnauthorizedException("Không có quyền sửa giải này");

        if (tournament.Status == TournamentStatus.Completed)
            throw new DomainException("Không thể sửa giải đã kết thúc");

        // Check locked fields
        var hasParticipants = await _participantRepo.CountConfirmedAsync(request.TournamentId, cancellationToken) > 0;
        if (hasParticipants && (request.Type != null || request.NumGroups.HasValue))
            throw new DomainException("Không thể thay đổi loại giải hoặc số bảng khi đã có người tham gia");

        if (request.Name != null) tournament.Name = request.Name.Trim();
        if (request.Description != null) tournament.Description = request.Description.Trim();
        if (request.Location != null) tournament.Location = request.Location.Trim();
        if (request.ScoringFormat != null)
            tournament.ScoringFormat = request.ScoringFormat == "best_of_1" ? ScoringFormat.BestOf1 : ScoringFormat.BestOf3;
        if (!string.IsNullOrEmpty(request.Date) && DateOnly.TryParse(request.Date, out var d))
            tournament.Date = d;

        _tournamentRepo.Update(tournament);
        await _uow.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new GetTournamentByIdQuery(tournament.Id, _currentUser.UserId), cancellationToken);
    }
}
