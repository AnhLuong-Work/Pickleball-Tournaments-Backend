namespace AppPickleball.Application.Features.Tournaments.DTOs;

public record TournamentDto(
    Guid Id, string Name, string? Description, string Type, int NumGroups,
    string ScoringFormat, string Status, string? Date, string? Location, string? BannerUrl,
    CreatorDto Creator, int ParticipantCount, int MaxParticipants, bool IsFull,
    DateTime CreatedAt, DateTime? UpdatedAt
);

public record TournamentDetailDto(
    Guid Id, string Name, string? Description, string Type, int NumGroups,
    string ScoringFormat, string Status, string? Date, string? Location, string? BannerUrl,
    CreatorDto Creator, int ParticipantCount, int MaxParticipants,
    CurrentUserTournamentDto? CurrentUser,
    List<GroupDetailDto> Groups,
    DateTime CreatedAt, DateTime? UpdatedAt
);

public record CreatorDto(Guid Id, string Name, string? AvatarUrl);
public record CurrentUserTournamentDto(string Role, string ParticipantStatus, Guid? GroupId, string? GroupName);
public record GroupDetailDto(Guid Id, string Name, List<GroupMemberDto> Members);
public record GroupMemberDto(Guid Id, string Name, string? AvatarUrl, decimal SkillLevel, int SeedOrder);
