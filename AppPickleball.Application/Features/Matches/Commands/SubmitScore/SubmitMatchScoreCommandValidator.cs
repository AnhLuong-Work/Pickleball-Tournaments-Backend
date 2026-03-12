using FluentValidation;

namespace AppPickleball.Application.Features.Matches.Commands.SubmitScore;

public class SubmitMatchScoreCommandValidator : AbstractValidator<SubmitMatchScoreCommand>
{
    public SubmitMatchScoreCommandValidator()
    {
        RuleFor(x => x.Player1Scores).NotNull().NotEmpty().WithMessage("Điểm Player 1 không được rỗng");
        RuleFor(x => x.Player2Scores).NotNull().NotEmpty().WithMessage("Điểm Player 2 không được rỗng");
        RuleFor(x => x).Must(x => x.Player1Scores.Length == x.Player2Scores.Length)
            .WithMessage("Số set Player 1 và Player 2 phải bằng nhau");
    }
}
