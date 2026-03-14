using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Users.Interfaces;
using MediatR;
using Shared.Kernel.Wrappers;
using FollowEntity = AppPickleball.Domain.Entities.Follow;

namespace AppPickleball.Application.Features.Users.Commands.Follow;

public class FollowUserCommandHandler : IRequestHandler<FollowUserCommand, ApiResponse<object>>
{
    private readonly IFollowRepository _followRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public FollowUserCommandHandler(IFollowRepository followRepo, IUserRepository userRepo,
        IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _followRepo = followRepo; _userRepo = userRepo;
        _uow = uow; _currentUser = currentUser;
    }

    public async Task<ApiResponse<object>> Handle(FollowUserCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == request.TargetUserId)
            throw new DomainException("Không thể tự follow chính mình");

        var targetUser = await _userRepo.GetByIdAsync(request.TargetUserId, cancellationToken)
            ?? throw new NotFoundException("Người dùng không tồn tại");

        if (await _followRepo.IsFollowingAsync(currentUserId, request.TargetUserId, cancellationToken))
            throw new DomainException("Bạn đã theo dõi người này rồi");

        await _followRepo.AddAsync(new FollowEntity { FollowerId = currentUserId, FollowingId = request.TargetUserId }, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.SuccessResponse(new { }, "Đã theo dõi", 204);
    }
}
