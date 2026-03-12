using AppPickleball.Domain.Common;
using AppPickleball.Domain.Enums;

namespace AppPickleball.Domain.Entities;

public class Match : BaseEntity
{
    public Guid TournamentId { get; set; }
    public Guid GroupId { get; set; }
    public int Round { get; set; }
    public int MatchOrder { get; set; }
    public Guid Player1Id { get; set; }  // UserId (singles) or TeamId (doubles)
    public Guid Player2Id { get; set; }
    public int[]? Player1Scores { get; set; }
    public int[]? Player2Scores { get; set; }
    public Guid? WinnerId { get; set; }
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;

    // Navigation
    public Tournament Tournament { get; set; } = default!;
    public Group Group { get; set; } = default!;
    public ICollection<MatchScoreHistory> ScoreHistories { get; set; } = new List<MatchScoreHistory>();
}
