using AppPickleball.Domain.Common;

namespace AppPickleball.Domain.Entities;

public class MatchScoreHistory : BaseCreatedEntity
{
    public Guid MatchId { get; set; }
    public Guid ModifiedBy { get; set; }
    public int[]? OldPlayer1Scores { get; set; }
    public int[]? OldPlayer2Scores { get; set; }
    public int[] NewPlayer1Scores { get; set; } = default!;
    public int[] NewPlayer2Scores { get; set; } = default!;
    public string? Reason { get; set; }

    // Navigation
    public Match Match { get; set; } = default!;
    public User ModifiedByUser { get; set; } = default!;
}
