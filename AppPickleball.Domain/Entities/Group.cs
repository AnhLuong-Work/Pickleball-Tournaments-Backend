namespace AppPickleball.Domain.Entities;

public class Group
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TournamentId { get; set; }
    public string Name { get; set; } = default!;
    public int DisplayOrder { get; set; }

    // Navigation
    public Tournament Tournament { get; set; } = default!;
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
}
