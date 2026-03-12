namespace Shared.Kernel.Wrappers;

public class PagedResponse<T> : ApiResponse<List<T>>
{
    public static PagedResponse<T> Create(
        List<T> data,
        int page,
        int limit,
        int totalItems,
        string message = "Success")
    {
        var totalPages = (int)Math.Ceiling(totalItems / (double)limit);

        return new PagedResponse<T>
        {
            Success = true,
            StatusCode = 200,
            Message = message,
            Data = data,
            Meta = new MetaResponse
            {
                Pagination = new PaginationMeta
                {
                    Page = page,
                    Limit = limit,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            }
        };
    }
}
