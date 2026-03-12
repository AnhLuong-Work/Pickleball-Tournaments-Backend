using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using System.Security.Claims;

namespace AppPickleball.Api.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                // Ưu tiên lấy từ claim (nếu token có UserId)
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(userIdClaim, out var userIdFromClaim))
                    return userIdFromClaim;

                // Nếu không có claim thì lấy từ header X-User-Id
                var userIdHeader = _httpContextAccessor.HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault();
                if (Guid.TryParse(userIdHeader, out var userIdFromHeader))
                    return userIdFromHeader;

                // Không có thì trả null (không ném exception, vì UserId có thể là tùy chọn)
                throw new InvalidOperationException("User ID is not available in the current context.");
            }
        }

    }

}
