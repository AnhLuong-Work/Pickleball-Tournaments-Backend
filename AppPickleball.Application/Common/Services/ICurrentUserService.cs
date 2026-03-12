namespace AppPickleball.Application.Common.Services;

// ICurrentUserService — lấy thông tin user hiện tại từ JWT claims
public interface ICurrentUserService
{
    Guid UserId { get; }
}
