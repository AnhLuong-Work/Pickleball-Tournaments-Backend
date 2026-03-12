using AppPickleball.Domain.Common;

namespace AppPickleball.Domain.Entities;

public class Team : BaseCreatedEntity
{
    public Guid TournamentId { get; set; }
    public string? Name { get; set; }
    public Guid Player1Id { get; set; }
    public Guid Player2Id { get; set; }

    // Navigation
    public Tournament Tournament { get; set; } = default!;
    public User Player1 { get; set; } = default!;
    public User Player2 { get; set; } = default!;
    public ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
}
