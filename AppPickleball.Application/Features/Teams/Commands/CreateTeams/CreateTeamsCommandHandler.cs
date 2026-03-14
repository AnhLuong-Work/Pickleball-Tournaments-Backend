using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Participants.Interfaces;
using AppPickleball.Application.Features.Teams.Interfaces;
using AppPickleball.Application.Features.Tournaments.Interfaces;
using AppPickleball.Application.Features.Teams.DTOs;
using AppPickleball.Domain.Entities;
using AppPickleball.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Teams.Commands.CreateTeams;

public class CreateTeamsCommandHandler : IRequestHandler<CreateTeamsCommand, ApiResponse<List<TeamDto>>>
{
    private readonly ITournamentRepository _tournamentRepo;
    private readonly ITeamRepository _teamRepo;
    private readonly IParticipantRepository _participantRepo;
    private readonly IBaseDbContext _db;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateTeamsCommandHandler(
        ITournamentRepository tournamentRepo,
        ITeamRepository teamRepo,
        IParticipantRepository participantRepo,
        IBaseDbContext db,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _tournamentRepo = tournamentRepo;
        _teamRepo = teamRepo;
        _participantRepo = participantRepo;
        _db = db;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<List<TeamDto>>> Handle(CreateTeamsCommand request, CancellationToken cancellationToken)
    {
        // 1. Load tournament, check exists + check creator
        var tournament = await _tournamentRepo.GetByIdAsync(request.TournamentId, cancellationToken)
            ?? throw new NotFoundException("Giải đấu không tồn tại");

        if (tournament.CreatorId != _currentUser.UserId)
            throw new UnauthorizedException("Chỉ người tạo giải mới có thể tạo teams");

        // 2. Validate tournament type == Doubles
        if (tournament.Type != TournamentType.Doubles)
            throw new DomainException("Chỉ có thể tạo teams cho giải Doubles");

        // 3. Validate tournament status == Open
        if (tournament.Status != TournamentStatus.Open)
            throw new DomainException("Chỉ có thể tạo teams khi giải đang ở trạng thái Open");

        // 4. Delete existing teams (replace)
        var existingTeams = await _teamRepo.GetByTournamentAsync(request.TournamentId, cancellationToken);
        foreach (var et in existingTeams)
            _teamRepo.Remove(et);

        // 5. Validate số lượng teams không vượt MaxParticipants / 2
        var maxTeams = tournament.NumGroups * 4; // NumGroups * 4 teams
        if (request.Teams.Count > maxTeams)
            throw new DomainException($"Số teams không được vượt quá {maxTeams} (NumGroups * 4)");

        // 6. Validate tất cả Player1Id/Player2Id là confirmed participants
        var confirmedParticipants = await _participantRepo.GetByTournamentAsync(
            request.TournamentId, ParticipantStatus.Confirmed, cancellationToken);
        var confirmedUserIds = confirmedParticipants.Select(p => p.UserId).ToHashSet();

        var allPlayerIds = request.Teams.SelectMany(t => new[] { t.Player1Id, t.Player2Id }).ToList();
        var nonParticipants = allPlayerIds.Where(id => !confirmedUserIds.Contains(id)).ToList();
        if (nonParticipants.Any())
            throw new DomainException($"Một số người chơi chưa được xác nhận tham gia giải đấu");

        // 7. Tạo Team entities
        var createdTeams = new List<Team>();
        foreach (var teamInput in request.Teams)
        {
            var team = new Team
            {
                TournamentId = request.TournamentId,
                Name = teamInput.Name,
                Player1Id = teamInput.Player1Id,
                Player2Id = teamInput.Player2Id
            };
            await _teamRepo.AddAsync(team, cancellationToken);
            createdTeams.Add(team);
        }

        await _uow.SaveChangesAsync(cancellationToken);

        // 8. Load user info
        var playerIds = allPlayerIds.Distinct().ToList();
        var users = await _db.Set<User>()
            .Where(u => playerIds.Contains(u.Id))
            .ToListAsync(cancellationToken);
        var userMap = users.ToDictionary(u => u.Id);

        var teamDtos = createdTeams.Select(team =>
        {
            userMap.TryGetValue(team.Player1Id, out var player1);
            userMap.TryGetValue(team.Player2Id, out var player2);
            return new TeamDto(
                team.Id,
                team.Name ?? string.Empty,
                team.Player1Id,
                player1?.Name ?? "Unknown",
                player1?.AvatarUrl,
                team.Player2Id,
                player2?.Name ?? "Unknown",
                player2?.AvatarUrl,
                team.CreatedAt
            );
        }).ToList();

        return ApiResponse<List<TeamDto>>.SuccessResponse(teamDtos, "Tạo teams thành công", 201);
    }
}
