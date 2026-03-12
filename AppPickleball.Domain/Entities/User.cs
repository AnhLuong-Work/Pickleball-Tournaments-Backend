using AppPickleball.Domain.Common;

namespace AppPickleball.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = default!;
    public string? PasswordHash { get; set; }
    public string Name { get; set; } = default!;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public decimal SkillLevel { get; set; } = 3.0m;
    public string? DominantHand { get; set; }
    public string? PaddleType { get; set; }
    public bool EmailVerified { get; set; } = false;
    public DateTime? EmailVerifiedAt { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }

    // Password Reset
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    // Navigation
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<UserAuthProvider> AuthProviders { get; set; } = new List<UserAuthProvider>();
    public ICollection<Tournament> CreatedTournaments { get; set; } = new List<Tournament>();
    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
    public ICollection<Follow> Following { get; set; } = new List<Follow>();
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();
}
