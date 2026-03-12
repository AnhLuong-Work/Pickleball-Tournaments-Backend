using AppPickleball.Application.Features.Users.Commands.Follow;
using AppPickleball.Application.Features.Users.Commands.Unfollow;
using AppPickleball.Application.Features.Users.Commands.UpdateProfile;
using AppPickleball.Application.Features.Users.DTOs;
using AppPickleball.Application.Features.Users.Queries.GetFollowers;
using AppPickleball.Application.Features.Users.Queries.GetFollowing;
using AppPickleball.Application.Features.Users.Queries.GetMyProfile;
using AppPickleball.Application.Features.Users.Queries.GetMyTournaments;
using AppPickleball.Application.Features.Users.Queries.GetUserMatches;
using AppPickleball.Application.Features.Users.Queries.GetUserProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernel.Wrappers;
using Swashbuckle.AspNetCore.Annotations;

namespace AppPickleball.Api.Controllers;

/// <summary>Quản lý profile người dùng, follow, lịch sử</summary>
[Route("api/users")]
[Authorize]
[SwaggerTag("User Profile — xem/cập nhật profile, follow, lịch sử giải đấu và trận đấu")]
[Produces("application/json")]
public class UserController : BaseApi.BaseApiController
{
    public UserController(IMediator mediator, ILogger<BaseApi.BaseApiController> logger)
        : base(mediator, logger) { }

    /// <summary>2.1 GET /users/me — Profile cá nhân</summary>
    [HttpGet("me")]
    [SwaggerOperation(
        Summary = "Lấy profile cá nhân",
        Description = "Trả về đầy đủ thông tin profile của người dùng hiện tại bao gồm thống kê: " +
                      "tổng giải đấu đã tham gia, số trận, số thắng/thua, win rate, số following/followers.")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMyProfileQuery(), ct);
        return Ok(result);
    }

    /// <summary>2.2 PUT /users/me — Cập nhật profile</summary>
    [HttpPut("me")]
    [SwaggerOperation(
        Summary = "Cập nhật profile cá nhân",
        Description = "Cập nhật thông tin profile. Tất cả fields đều optional — chỉ gửi field muốn thay đổi. " +
                      "skillLevel: số thực từ 1.0 đến 5.0 (mức kỹ năng Pickleball).")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateProfileCommand(request.Name, request.Bio, request.SkillLevel, request.DominantHand, request.PaddleType), ct);
        return Ok(result);
    }

    /// <summary>2.5 GET /users/me/following — Danh sách đang follow</summary>
    [HttpGet("me/following")]
    [SwaggerOperation(
        Summary = "Danh sách người đang follow",
        Description = "Trả về danh sách những người mà user hiện tại đang follow, kèm thông tin cơ bản.")]
    [ProducesResponseType(typeof(ApiResponse<List<FollowUserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFollowing(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetFollowingQuery(), ct);
        return Ok(result);
    }

    /// <summary>2.6 GET /users/me/followers — Danh sách followers</summary>
    [HttpGet("me/followers")]
    [SwaggerOperation(
        Summary = "Danh sách người đang follow mình",
        Description = "Trả về danh sách những người đang follow user hiện tại.")]
    [ProducesResponseType(typeof(ApiResponse<List<FollowUserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFollowers(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetFollowersQuery(), ct);
        return Ok(result);
    }

    /// <summary>2.4 GET /users/me/tournaments — Lịch sử giải đấu</summary>
    [HttpGet("me/tournaments")]
    [SwaggerOperation(
        Summary = "Lịch sử giải đấu của tôi",
        Description = "Danh sách giải đấu đã tham gia (status = Confirmed). " +
                      "Filter theo `status`: Draft, Open, Ready, InProgress, Completed, Cancelled.")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<UserTournamentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyTournaments(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMyTournamentsQuery(status, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>2.7 POST /users/:id/follow — Theo dõi người dùng</summary>
    [HttpPost("{id:guid}/follow")]
    [SwaggerOperation(
        Summary = "Follow người dùng",
        Description = "Bắt đầu theo dõi một người dùng khác. Không thể tự follow bản thân. " +
                      "Nếu đã follow rồi sẽ trả lỗi 409.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Follow(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new FollowUserCommand(id), ct);
        return StatusCode(204, result);
    }

    /// <summary>2.8 DELETE /users/:id/follow — Bỏ theo dõi</summary>
    [HttpDelete("{id:guid}/follow")]
    [SwaggerOperation(
        Summary = "Unfollow người dùng",
        Description = "Bỏ theo dõi một người dùng. Trả 404 nếu chưa follow.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unfollow(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new UnfollowUserCommand(id), ct);
        return StatusCode(204, result);
    }

    /// <summary>2.9 GET /users/:id — Profile công khai</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Xem profile công khai của người dùng",
        Description = "Trả về thông tin công khai của người dùng: stats, head-to-head với user hiện tại, " +
                      "trạng thái follow. Không cần đăng nhập, nhưng nếu có token sẽ trả thêm isFollowing/isFollowedBy/headToHead.")]
    [ProducesResponseType(typeof(ApiResponse<PublicUserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserProfile(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserProfileQuery(id), ct);
        return Ok(result);
    }

    /// <summary>2.10 GET /users/:id/matches — Lịch sử trận đấu</summary>
    [HttpGet("{id:guid}/matches")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Lịch sử trận đấu của người dùng",
        Description = "Danh sách các trận đấu đã hoàn thành của một user. Không cần đăng nhập. " +
                      "Kết quả trả về dưới góc nhìn của user đó (MyScores, OpponentScores, Won).")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<UserMatchDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserMatches(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetUserMatchesQuery(id, page, pageSize), ct);
        return Ok(result);
    }
}

public record UpdateProfileRequest(string? Name, string? Bio, decimal? SkillLevel, string? DominantHand, string? PaddleType);
