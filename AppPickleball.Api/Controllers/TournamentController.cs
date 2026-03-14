using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Participants.Commands.CreateGroups;
using AppPickleball.Application.Features.Participants.Commands.InviteParticipants;
using AppPickleball.Application.Features.Participants.Commands.RequestJoin;
using AppPickleball.Application.Features.Participants.Commands.RespondToRequest;
using AppPickleball.Application.Features.Participants.DTOs;
using AppPickleball.Application.Features.Participants.Queries.GetParticipants;
using AppPickleball.Application.Features.Teams.Commands.CreateTeams;
using AppPickleball.Application.Features.Teams.DTOs;
using AppPickleball.Application.Features.Tournaments.Commands.CancelTournament;
using AppPickleball.Application.Features.Tournaments.Commands.CreateTournament;
using AppPickleball.Application.Features.Tournaments.Commands.UpdateTournament;
using AppPickleball.Application.Features.Tournaments.Commands.UpdateTournamentStatus;
using AppPickleball.Application.Features.Tournaments.DTOs;
using AppPickleball.Application.Features.Tournaments.Queries.GetTournamentById;
using AppPickleball.Application.Features.Tournaments.Queries.GetTournaments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernel.Wrappers;
using Swashbuckle.AspNetCore.Annotations;

namespace AppPickleball.Api.Controllers;

/// <summary>Quản lý giải đấu, người tham gia và bảng đấu</summary>
[Route("api/tournaments")]
[Authorize]
[SwaggerTag("Tournament — tạo/quản lý giải đấu, mời người chơi, xếp bảng, tạo teams")]
[Produces("application/json")]
public class TournamentController : BaseApi.BaseApiController
{
    private readonly ICurrentUserService _currentUser;

    public TournamentController(IMediator mediator, ILogger<BaseApi.BaseApiController> logger, ICurrentUserService currentUser)
        : base(mediator, logger)
    {
        _currentUser = currentUser;
    }

    // ===== TOURNAMENT ENDPOINTS =====

    /// <summary>3.1 GET /tournaments — Danh sách giải đấu</summary>
    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Danh sách giải đấu",
        Description = "Lấy danh sách giải đấu công khai (không bao gồm Draft và Cancelled). " +
                      "Hỗ trợ filter theo search/type/status và phân trang. " +
                      "type: Singles | Doubles. status: Open | Ready | InProgress | Completed.")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<TournamentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTournaments(
        [FromQuery] string? search, [FromQuery] string? type, [FromQuery] string? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "createdAt", [FromQuery] string sortOrder = "desc",
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetTournamentsQuery(search, type, status, page, pageSize, sortBy, sortOrder), ct);
        return Ok(result);
    }

    /// <summary>3.2 POST /tournaments — Tạo giải đấu</summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Tạo giải đấu mới",
        Description = "Tạo giải đấu mới với status = Draft. Yêu cầu tài khoản đã xác thực email. " +
                      "type: Singles (cá nhân) | Doubles (đôi). " +
                      "numGroups: số bảng (1-8). scoringFormat: BestOf1 | BestOf3.")]
    [ProducesResponseType(typeof(ApiResponse<TournamentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateTournament([FromBody] CreateTournamentRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateTournamentCommand(request.Name, request.Description, request.Type, request.NumGroups,
                request.ScoringFormat, request.Date, request.Location), ct);
        return result.Success ? StatusCode(201, result) : BadRequest(result);
    }

    /// <summary>3.3 GET /tournaments/:id — Chi tiết giải đấu</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Chi tiết giải đấu",
        Description = "Trả về đầy đủ thông tin giải đấu. Groups chỉ hiển thị khi status >= Ready. " +
                      "Nếu có token, trả thêm `currentUser` (role, participantStatus, group hiện tại).")]
    [ProducesResponseType(typeof(ApiResponse<TournamentDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTournament(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTournamentByIdQuery(id, _currentUser.UserId), ct);
        return Ok(result);
    }

    /// <summary>3.4 PUT /tournaments/:id — Cập nhật giải</summary>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        Summary = "Cập nhật thông tin giải đấu",
        Description = "Cập nhật thông tin giải đấu. Chỉ creator mới được phép. " +
                      "Không thể thay đổi `type` và `numGroups` khi đã có người tham gia confirmed.")]
    [ProducesResponseType(typeof(ApiResponse<TournamentDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTournament(Guid id, [FromBody] UpdateTournamentRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateTournamentCommand(id, request.Name, request.Description, request.Type, request.NumGroups,
                request.ScoringFormat, request.Date, request.Location), ct);
        return Ok(result);
    }

    /// <summary>3.5 DELETE /tournaments/:id — Hủy giải</summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Hủy giải đấu",
        Description = "Chuyển status giải đấu sang Cancelled. Chỉ creator được phép. " +
                      "Yêu cầu `reason` khi giải đang InProgress.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelTournament(Guid id, [FromBody] CancelTournamentRequest? request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelTournamentCommand(id, request?.Reason), ct);
        return result.Success ? StatusCode(204, result) : BadRequest(result);
    }

    /// <summary>3.6 PATCH /tournaments/:id/status — Chuyển trạng thái</summary>
    [HttpPut("{id:guid}/status")]
    [SwaggerOperation(
        Summary = "Chuyển trạng thái giải đấu",
        Description = "State machine: Draft → Open → Ready → InProgress → Completed. " +
                      "Open→Ready: phải đủ người confirmed + đã xếp bảng + đã tạo lịch thi đấu. " +
                      "InProgress→Completed: tự động khi tất cả matches đã Complete.")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateTournamentStatusCommand(id, request.Status), ct);
        return Ok(result);
    }

    // ===== PARTICIPANT ENDPOINTS =====

    /// <summary>4.4 GET /tournaments/:id/participants — Danh sách người tham gia</summary>
    [HttpGet("{id:guid}/participants")]
    [SwaggerOperation(
        Summary = "Danh sách người tham gia giải",
        Description = "Trả về các danh sách: confirmed (đã xác nhận), invited (đã mời, chờ chấp nhận), " +
                      "pending (đang chờ duyệt). Filter theo `status`: confirmed | invited | pending.")]
    [ProducesResponseType(typeof(ApiResponse<ParticipantListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetParticipants(Guid id, [FromQuery] string? status, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetParticipantsQuery(id, status), ct);
        return Ok(result);
    }

    /// <summary>4.2 POST /tournaments/:id/join — Xin tham gia giải</summary>
    [HttpPost("{id:guid}/join")]
    [SwaggerOperation(
        Summary = "Xin tham gia giải đấu",
        Description = "Gửi yêu cầu tham gia giải đấu. Chỉ có thể khi giải đang Open và chưa đầy. " +
                      "Creator sẽ duyệt qua `/tournaments/:id/participants/:pid/respond`. " +
                      "Status sau khi gửi: RequestPending.")]
    [ProducesResponseType(typeof(ApiResponse<ParticipantDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RequestJoin(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new RequestJoinTournamentCommand(id), ct);
        return result.Success ? StatusCode(201, result) : BadRequest(result);
    }

    /// <summary>4.3 POST /tournaments/:id/invite — Mời người chơi</summary>
    [HttpPost("{id:guid}/invite")]
    [SwaggerOperation(
        Summary = "Mời người chơi tham gia giải",
        Description = "Creator mời nhiều người chơi cùng lúc (tối đa 20). " +
                      "Trả partial success: danh sách invited thành công và errors cho các userId thất bại. " +
                      "Status sau khi mời: InvitedPending.")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> InviteParticipants(Guid id, [FromBody] InviteRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new InviteParticipantsCommand(id, request.UserIds), ct);
        return Ok(result);
    }

    /// <summary>4.5 PUT /tournaments/:id/participants/:pid/respond — Duyệt/từ chối yêu cầu</summary>
    [HttpPut("{id:guid}/participants/{pid:guid}/respond")]
    [SwaggerOperation(
        Summary = "Duyệt hoặc từ chối yêu cầu tham gia",
        Description = "Creator duyệt (approve) hoặc từ chối (reject) yêu cầu tham gia. " +
                      "action: 'approve' → status Confirmed + ghi nhận JoinedAt. " +
                      "action: 'reject' → status Rejected + lưu reason (optional).")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RespondToRequest(Guid id, Guid pid, [FromBody] RespondRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RespondToParticipantRequestCommand(id, pid, request.Action, request.Reason), ct);
        return Ok(result);
    }

    /// <summary>4.7 POST /tournaments/:id/groups — Xếp bảng và tạo lịch thi đấu</summary>
    [HttpPost("{id:guid}/groups")]
    [SwaggerOperation(
        Summary = "Xếp bảng và tạo lịch thi đấu Round Robin",
        Description = "mode=random: preview xếp bảng ngẫu nhiên (không lưu DB). " +
                      "mode=manual: xác nhận xếp bảng, tạo Groups + GroupMembers + Matches (Round Robin: 3 rounds, 6 matches/group). " +
                      "Mỗi bảng phải có đúng 4 thành viên, không được trùng người.")]
    [ProducesResponseType(typeof(ApiResponse<CreateGroupsResultDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateGroups(Guid id, [FromBody] CreateGroupsRequest request, CancellationToken ct)
    {
        var groupInputs = request.Groups?.Select(g => new GroupInput(g.Name, g.MemberIds)).ToList();
        var result = await _mediator.Send(new CreateGroupsCommand(id, request.Mode, groupInputs), ct);
        return result.Success ? StatusCode(201, result) : BadRequest(result);
    }

    /// <summary>4.6 POST /tournaments/:id/teams — Tạo teams cho Doubles</summary>
    [HttpPost("{id:guid}/teams")]
    [SwaggerOperation(
        Summary = "Tạo teams cho giải Doubles",
        Description = "Tạo các team gồm 2 người cho giải đấu Doubles. Chỉ Creator được phép. " +
                      "Giải phải ở trạng thái Open. Tất cả Player1Id/Player2Id phải là Confirmed participants. " +
                      "Gọi endpoint này sẽ xóa và thay thế toàn bộ teams hiện có.")]
    [ProducesResponseType(typeof(ApiResponse<List<TeamDto>>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTeams(Guid id, [FromBody] CreateTeamsRequest request, CancellationToken ct)
    {
        var teams = request.Teams.Select(t => new TeamInput(t.Name, t.Player1Id, t.Player2Id)).ToList();
        var result = await _mediator.Send(new CreateTeamsCommand(id, teams), ct);
        return result.Success ? StatusCode(201, result) : BadRequest(result);
    }
}
