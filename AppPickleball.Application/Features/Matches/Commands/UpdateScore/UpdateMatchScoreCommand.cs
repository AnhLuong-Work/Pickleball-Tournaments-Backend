using AppPickleball.Application.Features.Matches.DTOs;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Matches.Commands.UpdateScore;

public record UpdateMatchScoreCommand(
    Guid MatchId, int[] Player1Scores, int[] Player2Scores, string? Reason
) : IRequest<ApiResponse<MatchDto>>;
