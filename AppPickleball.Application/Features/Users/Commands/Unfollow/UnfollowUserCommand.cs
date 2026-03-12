using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Users.Commands.Unfollow;

public record UnfollowUserCommand(Guid TargetUserId) : IRequest<ApiResponse<object>>;
