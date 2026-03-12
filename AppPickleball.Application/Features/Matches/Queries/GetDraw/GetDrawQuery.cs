using MediatR;
using Shared.Kernel.Wrappers;
using AppPickleball.Application.Features.Matches.DTOs;

namespace AppPickleball.Application.Features.Matches.Queries.GetDraw;

public record GetDrawQuery(Guid TournamentId) : IRequest<ApiResponse<DrawDto>>;
