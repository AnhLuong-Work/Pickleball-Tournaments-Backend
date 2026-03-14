using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Users.Interfaces;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Users.Commands.Unfollow;

public class UnfollowUserCommandHandler : IRequestHandler<UnfollowUserCommand, ApiResponse<object>>
{
    private readonly IFollowRepository _followRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UnfollowUserCommandHandler(IFollowRepository followRepo, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _followRepo = followRepo; _uow = uow; _currentUser = currentUser;
    }

    public async Task<ApiResponse<object>> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
    {
        var follow = await _followRepo.GetByPairAsync(_currentUser.UserId, request.TargetUserId, cancellationToken)
            ?? throw new NotFoundException("Bạn chưa theo dõi người này");

        _followRepo.Remove(follow);
        await _uow.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.SuccessResponse(new { }, "Đã bỏ theo dõi", 204);
    }
}
