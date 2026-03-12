using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Users.Commands.Follow;

public record FollowUserCommand(Guid TargetUserId) : IRequest<ApiResponse<object>>;
