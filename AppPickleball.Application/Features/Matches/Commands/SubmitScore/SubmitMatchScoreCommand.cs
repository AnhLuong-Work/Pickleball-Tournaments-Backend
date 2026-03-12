using AppPickleball.Application.Features.Matches.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Matches.Commands.SubmitScore;

public record SubmitMatchScoreCommand(
    Guid MatchId, int[] Player1Scores, int[] Player2Scores, string? Reason
) : IRequest<ApiResponse<MatchDto>>;
