using AppPickleball.Domain.Common;

namespace AppPickleball.Domain.Entities;

public class Follow : BaseCreatedEntity
{
    public Guid FollowerId { get; set; }
    public Guid FollowingId { get; set; }

    // Navigation
    public User Follower { get; set; } = default!;
    public User FollowingUser { get; set; } = default!;
}
