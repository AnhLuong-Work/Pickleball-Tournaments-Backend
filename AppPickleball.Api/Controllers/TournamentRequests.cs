namespace AppPickleball.Api.Controllers;

public record CreateTournamentRequest(string Name, string? Description, string Type, int NumGroups, string? ScoringFormat, string? Date, string? Location);
public record UpdateTournamentRequest(string? Name, string? Description, string? Type, int? NumGroups, string? ScoringFormat, string? Date, string? Location);
public record CancelTournamentRequest(string? Reason);
public record UpdateStatusRequest(string Status);
public record InviteRequest(List<Guid> UserIds);
public record RespondRequest(string Action, string? Reason);
public record CreateGroupsRequest(string Mode, List<GroupInputRequest>? Groups);
public record GroupInputRequest(string Name, List<Guid> MemberIds);
public record CreateTeamsRequest(List<TeamInputRequest> Teams);
public record TeamInputRequest(string Name, Guid Player1Id, Guid Player2Id);
