using FluentValidation;

namespace AppPickleball.Application.Features.Tournaments.Commands.CreateTournament;

public class CreateTournamentCommandValidator : AbstractValidator<CreateTournamentCommand>
{
    public CreateTournamentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description != null);
        RuleFor(x => x.Type).NotEmpty().Must(t => t == "singles" || t == "doubles")
            .WithMessage("Type phải là 'singles' hoặc 'doubles'");
        RuleFor(x => x.NumGroups).InclusiveBetween(1, 4)
            .When(x => x.Type == "singles");
        RuleFor(x => x.NumGroups).InclusiveBetween(1, 2)
            .When(x => x.Type == "doubles");
        RuleFor(x => x.Location).MaximumLength(500).When(x => x.Location != null);
    }
}
