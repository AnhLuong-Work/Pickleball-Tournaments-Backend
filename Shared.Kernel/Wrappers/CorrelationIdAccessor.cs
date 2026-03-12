namespace Shared.Kernel.Wrappers;

/// <summary>
/// Lưu trữ CorrelationId cho request hiện tại.
/// Middleware set giá trị → MetaResponse đọc giá trị.
/// Dùng AsyncLocal để thread-safe (mỗi request có giá trị riêng).
/// </summary>
public static class CorrelationIdAccessor
{
    private static readonly AsyncLocal<string?> _correlationId = new();

    public static string? CorrelationId
    {
        get => _correlationId.Value;
        set => _correlationId.Value = value;
    }

    /// <summary>
    /// Lấy CorrelationId hiện tại, nếu chưa set thì tạo GUID mới.
    /// </summary>
    public static string GetOrCreate()
        => _correlationId.Value ??= Guid.NewGuid().ToString();
}
