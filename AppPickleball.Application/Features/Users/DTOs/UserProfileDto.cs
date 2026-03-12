namespace AppPickleball.Application.Features.Users.DTOs;

public record UserProfileDto(
    Guid Id, string Email, string Name, string? AvatarUrl, string? Bio,
    decimal SkillLevel, string? DominantHand, string? PaddleType,
    bool EmailVerified, UserStatsDto Stats, DateTime CreatedAt
);

public record UserStatsDto(
    int TotalTournaments, int TotalMatches, int Wins, int Losses,
    double WinRate, int FollowingCount, int FollowersCount
);

public record PublicUserProfileDto(
    Guid Id, string Name, string? AvatarUrl, string? Bio,
    decimal SkillLevel, string? DominantHand, string? PaddleType,
    UserStatsDto Stats, bool IsFollowing, bool IsFollowedBy,
    HeadToHeadDto? HeadToHead
);

public record HeadToHeadDto(int TotalMatches, int MyWins, int TheirWins, DateOnly? LastPlayed);

public record FollowUserDto(Guid UserId, string Name, string? AvatarUrl, decimal SkillLevel, DateTime FollowedAt);

public record UserTournamentDto(Guid TournamentId, string Name, string Type, string Status, string? Date, string? Location, string? BannerUrl, string ParticipantRole, DateTime? JoinedAt);

public record UserMatchDto(Guid MatchId, Guid TournamentId, string TournamentName, string GroupName, int Round, int MatchOrder, Guid OpponentId, string OpponentName, string? OpponentAvatarUrl, int[]? MyScores, int[]? OpponentScores, bool Won, DateTime PlayedAt);
