namespace AppPickleball.Domain.Common;

// Base entity cho tất cả entities có đầy đủ audit fields (UUID PK, soft delete, created/updated/deleted by)
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public Guid? DeletedBy { get; set; }

    public bool IsDeleted => DeletedAt.HasValue;
}

// Base entity nhẹ, chỉ có CreatedAt (dùng cho junction tables, token tables, log tables)
public abstract class BaseCreatedEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
