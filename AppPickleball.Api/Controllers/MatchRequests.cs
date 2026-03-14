namespace AppPickleball.Api.Controllers;

public record ScoreRequest(int[] Player1Scores, int[] Player2Scores, string? Reason);
