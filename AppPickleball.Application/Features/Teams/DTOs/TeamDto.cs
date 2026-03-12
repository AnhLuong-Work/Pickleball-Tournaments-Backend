namespace AppPickleball.Application.Features.Teams.DTOs;

public record TeamDto(
    Guid Id,
    string Name,
    Guid Player1Id,
    string Player1Name,
    string? Player1AvatarUrl,
    Guid Player2Id,
    string Player2Name,
    string? Player2AvatarUrl,
    DateTime CreatedAt
);
