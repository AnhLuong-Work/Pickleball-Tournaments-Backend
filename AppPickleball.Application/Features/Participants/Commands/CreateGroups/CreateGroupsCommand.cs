using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Participants.Commands.CreateGroups;

public record CreateGroupsCommand(Guid TournamentId, string Mode, List<GroupInput>? Groups) : IRequest<ApiResponse<CreateGroupsResultDto>>;
public record GroupInput(string Name, List<Guid> MemberIds);
public record CreateGroupsResultDto(bool Saved, List<GroupResultDto> Groups, List<MatchResultDto> Matches, int TotalMatches);
public record GroupResultDto(Guid Id, string Name, List<MemberResultDto> Members);
public record MemberResultDto(Guid Id, string Name, string? AvatarUrl, decimal SkillLevel, int SeedOrder);
public record MatchResultDto(Guid Id, string GroupName, int Round, int MatchOrder, Guid Player1Id, Guid Player2Id, string Status);
