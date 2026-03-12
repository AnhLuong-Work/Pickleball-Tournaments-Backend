using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Tournaments.DTOs;
using AppPickleball.Application.Features.Tournaments.Queries.GetTournamentById;
using AppPickleball.Domain.Entities;
using AppPickleball.Domain.Enums;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Tournaments.Commands.CreateTournament;

public class CreateTournamentCommandHandler : IRequestHandler<CreateTournamentCommand, ApiResponse<TournamentDetailDto>>
{
    private readonly ITournamentRepository _tournamentRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IMediator _mediator;

    public CreateTournamentCommandHandler(ITournamentRepository tournamentRepo, IUserRepository userRepo,
        IUnitOfWork uow, ICurrentUserService currentUser, IMediator mediator)
    {
        _tournamentRepo = tournamentRepo; _userRepo = userRepo;
        _uow = uow; _currentUser = currentUser; _mediator = mediator;
    }

    public async Task<ApiResponse<TournamentDetailDto>> Handle(CreateTournamentCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("User không tồn tại");

        if (!user.EmailVerified)
            throw new DomainException("Bạn cần xác thực email trước khi tạo giải đấu");

        var type = request.Type == "singles" ? TournamentType.Singles : TournamentType.Doubles;
        var scoringFormat = request.ScoringFormat == "best_of_1" ? ScoringFormat.BestOf1 : ScoringFormat.BestOf3;

        DateOnly? date = null;
        if (!string.IsNullOrEmpty(request.Date) && DateOnly.TryParse(request.Date, out var parsedDate))
        {
            if (parsedDate < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new DomainException("Ngày thi đấu phải trong tương lai");
            date = parsedDate;
        }

        var tournament = new Tournament
        {
            CreatorId = _currentUser.UserId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Type = type,
            NumGroups = request.NumGroups,
            ScoringFormat = scoringFormat,
            Status = TournamentStatus.Draft,
            Date = date,
            Location = request.Location?.Trim()
        };

        await _tournamentRepo.AddAsync(tournament, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var result = await _mediator.Send(new GetTournamentByIdQuery(tournament.Id, _currentUser.UserId), cancellationToken);
        return ApiResponse<TournamentDetailDto>.SuccessResponse(result.Data!, "Tạo giải đấu thành công", 201);
    }
}
