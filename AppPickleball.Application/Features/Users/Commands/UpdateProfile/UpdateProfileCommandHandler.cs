using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Users.DTOs;
using AppPickleball.Application.Features.Users.Queries.GetMyProfile;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, ApiResponse<UserProfileDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IMediator _mediator;

    public UpdateProfileCommandHandler(IUserRepository userRepo, IUnitOfWork uow,
        ICurrentUserService currentUser, IMediator mediator)
    {
        _userRepo = userRepo; _uow = uow; _currentUser = currentUser; _mediator = mediator;
    }

    public async Task<ApiResponse<UserProfileDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("User không tồn tại");

        if (request.Name != null) user.Name = request.Name.Trim();
        if (request.Bio != null) user.Bio = request.Bio.Trim();
        if (request.SkillLevel.HasValue) user.SkillLevel = request.SkillLevel.Value;
        if (request.DominantHand != null) user.DominantHand = request.DominantHand;
        if (request.PaddleType != null) user.PaddleType = request.PaddleType.Trim();

        _userRepo.Update(user);
        await _uow.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new GetMyProfileQuery(), cancellationToken);
    }
}
