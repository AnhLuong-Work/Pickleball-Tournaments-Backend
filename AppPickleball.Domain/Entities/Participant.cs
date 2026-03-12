using AppPickleball.Domain.Common;
using AppPickleball.Domain.Enums;

namespace AppPickleball.Domain.Entities;

public class Participant : BaseCreatedEntity
{
    public Guid TournamentId { get; set; }
    public Guid UserId { get; set; }
    public ParticipantStatus Status { get; set; } = ParticipantStatus.RequestPending;
    public Guid? InvitedBy { get; set; }
    public string? RejectReason { get; set; }
    public DateTime? JoinedAt { get; set; }

    // Navigation
    public Tournament Tournament { get; set; } = default!;
    public User User { get; set; } = default!;
    public User? InvitedByUser { get; set; }
}
