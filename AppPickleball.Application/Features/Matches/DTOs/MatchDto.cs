namespace AppPickleball.Application.Features.Matches.DTOs;

public record MatchDto(
    Guid Id, string GroupName, int Round, int MatchOrder,
    Guid Player1Id, Guid Player2Id,
    int[]? Player1Scores, int[]? Player2Scores,
    Guid? WinnerId, string Status
);

public record MatchDetailDto(
    Guid Id, Guid GroupId, string GroupName, int Round, int MatchOrder,
    Guid Player1Id, string Player1Name, string? Player1AvatarUrl,
    Guid Player2Id, string Player2Name, string? Player2AvatarUrl,
    int[]? Player1Scores, int[]? Player2Scores,
    Guid? WinnerId, string Status
);

public record StandingDto(
    Guid PlayerId, string PlayerName, string? PlayerAvatarUrl,
    int Played, int Wins, int Losses, int SetsWon, int SetsLost, int Points
);

public record TournamentResultDto(
    Guid TournamentId, string TournamentName, string Status,
    List<GroupResultStandingDto> GroupResults
);

public record GroupResultStandingDto(
    Guid GroupId, string GroupName, List<StandingDto> Standings
);

public record DrawDto(
    Guid TournamentId, string TournamentName, string Type, string Status,
    List<DrawGroupDto> Groups
);

public record DrawGroupDto(
    Guid GroupId, string GroupName, int DisplayOrder,
    List<DrawMemberDto> Members,
    List<DrawMatchDto> Matches
);

public record DrawMemberDto(Guid Id, string Name, string? AvatarUrl, int SeedOrder);

public record DrawMatchDto(
    Guid MatchId, int Round, int MatchOrder,
    Guid Player1Id, string Player1Name, string? Player1AvatarUrl,
    Guid Player2Id, string Player2Name, string? Player2AvatarUrl,
    int[]? Player1Scores, int[]? Player2Scores,
    Guid? WinnerId, string Status
);
