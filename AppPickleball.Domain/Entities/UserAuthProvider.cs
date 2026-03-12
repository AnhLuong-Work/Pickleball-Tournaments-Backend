using AppPickleball.Domain.Common;

namespace AppPickleball.Domain.Entities;

public class UserAuthProvider : BaseCreatedEntity
{
    public Guid UserId { get; set; }
    public string Provider { get; set; } = default!; // google, facebook, apple
    public string ProviderUserId { get; set; } = default!;
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }

    // Navigation
    public User User { get; set; } = default!;
}
