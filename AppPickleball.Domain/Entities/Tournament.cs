using AppPickleball.Domain.Common;
using AppPickleball.Domain.Enums;

namespace AppPickleball.Domain.Entities;

public class Tournament : BaseEntity
{
    public Guid CreatorId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public TournamentType Type { get; set; }
    public int NumGroups { get; set; }
    public ScoringFormat ScoringFormat { get; set; } = ScoringFormat.BestOf3;
    public TournamentStatus Status { get; set; } = TournamentStatus.Draft;
    public DateOnly? Date { get; set; }
    public string? Location { get; set; }
    public string? BannerUrl { get; set; }

    // Navigation
    public User Creator { get; set; } = default!;
    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
    public ICollection<Group> Groups { get; set; } = new List<Group>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();

    // Domain helpers
    public int MaxParticipants => Type == TournamentType.Singles
        ? NumGroups * 4
        : NumGroups * 4 * 2;
}
