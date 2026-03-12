namespace Shared.Kernel.Wrappers;

public class MetaResponse
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = CorrelationIdAccessor.GetOrCreate();
    public PaginationMeta? Pagination { get; set; }
}
