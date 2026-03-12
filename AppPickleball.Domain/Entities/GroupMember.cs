namespace AppPickleball.Domain.Entities;

public class GroupMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GroupId { get; set; }
    public Guid? PlayerId { get; set; }  // Singles: UserId
    public Guid? TeamId { get; set; }    // Doubles: TeamId
    public int SeedOrder { get; set; }

    // Navigation
    public Group Group { get; set; } = default!;
    public User? Player { get; set; }
    public Team? Team { get; set; }
}
