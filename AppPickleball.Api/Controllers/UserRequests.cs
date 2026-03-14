namespace AppPickleball.Api.Controllers;

public record UpdateProfileRequest(string? Name, string? Bio, decimal? SkillLevel, string? DominantHand, string? PaddleType);
