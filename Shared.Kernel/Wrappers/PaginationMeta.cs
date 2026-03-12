namespace Shared.Kernel.Wrappers;

public class PaginationMeta
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}
