using AppPickleball.Application.Features.Matches.Commands.SubmitScore;
using AppPickleball.Application.Features.Matches.Commands.UpdateScore;
using AppPickleball.Application.Features.Matches.DTOs;
using AppPickleball.Application.Features.Matches.Queries.GetDraw;
using AppPickleball.Application.Features.Matches.Queries.GetGroupStandings;
using AppPickleball.Application.Features.Matches.Queries.GetMatches;
using AppPickleball.Application.Features.Matches.Queries.GetTournamentResults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernel.Wrappers;
using Swashbuckle.AspNetCore.Annotations;

namespace AppPickleball.Api.Controllers;

/// <summary>Quản lý trận đấu, nhập điểm và kết quả</summary>
[Authorize]
[Route("api")]
[SwaggerTag("Match & Scoring — lịch thi đấu, nhập/sửa điểm, bảng xếp hạng, kết quả giải")]
[Produces("application/json")]
public class MatchController : BaseApi.BaseApiController
{
    public MatchController(IMediator mediator, ILogger<BaseApi.BaseApiController> logger)
        : base(mediator, logger) { }

    /// <summary>5.2 GET /tournaments/:id/draw — Bracket/lịch thi đấu tổng quan</summary>
    [HttpGet("tournaments/{id:guid}/draw")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Xem bracket / lịch thi đấu đầy đủ",
        Description = "Trả về toàn bộ cấu trúc giải đấu: từng bảng với danh sách thành viên và lịch thi đấu chi tiết. " +
                      "Không cần đăng nhập. Chỉ có dữ liệu khi status >= Ready. " +
                      "Với giải Doubles: Player1Id/Player2Id là TeamId.")]
    [ProducesResponseType(typeof(ApiResponse<DrawDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDraw(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDrawQuery(id), ct);
        return Ok(result);
    }

    /// <summary>5.1 GET /tournaments/:id/matches — Lịch thi đấu</summary>
    [HttpGet("tournaments/{id:guid}/matches")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Danh sách trận đấu của giải",
        Description = "Lấy tất cả trận đấu trong một giải, kèm tên bảng. " +
                      "Status: Scheduled | InProgress | Completed | Walkover.")]
    [ProducesResponseType(typeof(ApiResponse<List<MatchDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMatches(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMatchesQuery(id), ct);
        return Ok(result);
    }

    /// <summary>5.3 POST /matches/:id/score — Nhập điểm trận đấu</summary>
    [HttpPost("matches/{id:guid}/score")]
    [SwaggerOperation(
        Summary = "Nhập điểm trận đấu",
        Description = "Nhập kết quả điểm số cho trận đấu. Chỉ Creator của giải được phép. " +
                      "BestOf1: truyền 1 set [score1, score2]. BestOf3: truyền 2-3 sets. " +
                      "Winner được xác định tự động từ điểm số. " +
                      "Nếu tất cả trận đã Complete, giải tự động chuyển sang Completed.")]
    [ProducesResponseType(typeof(ApiResponse<MatchDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitScore(Guid id, [FromBody] ScoreRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SubmitMatchScoreCommand(id, request.Player1Scores, request.Player2Scores, request.Reason), ct);
        return result.Success ? StatusCode(201, result) : BadRequest(result);
    }

    /// <summary>5.4 PUT /matches/:id/score — Sửa điểm trận đấu</summary>
    [HttpPut("matches/{id:guid}/score")]
    [SwaggerOperation(
        Summary = "Sửa điểm trận đấu đã nhập",
        Description = "Cập nhật lại điểm số của trận đấu đã Complete. Chỉ Creator được phép. " +
                      "Lịch sử chỉnh sửa được lưu lại trong MatchScoreHistory. " +
                      "Nên cung cấp `reason` để ghi nhận lý do sửa điểm.")]
    [ProducesResponseType(typeof(ApiResponse<MatchDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateScore(Guid id, [FromBody] ScoreRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateMatchScoreCommand(id, request.Player1Scores, request.Player2Scores, request.Reason), ct);
        return Ok(result);
    }

    /// <summary>5.5 GET /tournaments/:id/groups/:gid/standings — BXH bảng</summary>
    [HttpGet("tournaments/{id:guid}/groups/{gid:guid}/standings")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Bảng xếp hạng của một bảng đấu",
        Description = "Tính toán và trả về bảng xếp hạng trong một group. " +
                      "Thứ tự: Points giảm dần (Win=3pts) → SetsWon/SetsLost ratio → tên. " +
                      "Chỉ tính các trận đã Completed.")]
    [ProducesResponseType(typeof(ApiResponse<List<StandingDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGroupStandings(Guid id, Guid gid, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetGroupStandingsQuery(id, gid), ct);
        return Ok(result);
    }

    /// <summary>5.6 GET /tournaments/:id/results — Kết quả tổng thể giải đấu</summary>
    [HttpGet("tournaments/{id:guid}/results")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Kết quả tổng thể giải đấu",
        Description = "Trả về kết quả tổng hợp toàn giải: bảng xếp hạng tất cả các bảng, " +
                      "cùng cờ `isComplete` cho biết giải đã kết thúc hay chưa.")]
    [ProducesResponseType(typeof(ApiResponse<TournamentResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResults(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTournamentResultsQuery(id), ct);
        return Ok(result);
    }
}
