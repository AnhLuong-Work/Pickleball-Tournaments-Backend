namespace AppPickleball.Application.Features.Participants.DTOs;

public record ParticipantDto(
    Guid Id, UserBriefDto User, string Status,
    DateTime? JoinedAt, DateTime CreatedAt
);

public record UserBriefDto(Guid Id, string Name, string? AvatarUrl, decimal SkillLevel);

public record ParticipantListDto(
    List<ParticipantDto> Data,
    int TotalConfirmed, int TotalInvitedPending, int TotalRequestPending, int MaxParticipants
);
