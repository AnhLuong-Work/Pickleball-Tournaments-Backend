using FluentValidation;

namespace AppPickleball.Application.Features.Teams.Commands.CreateTeams;

public class CreateTeamsCommandValidator : AbstractValidator<CreateTeamsCommand>
{
    public CreateTeamsCommandValidator()
    {
        RuleFor(x => x.Teams)
            .NotEmpty().WithMessage("Danh sách teams không được để trống");

        RuleForEach(x => x.Teams).ChildRules(team =>
        {
            team.RuleFor(t => t.Name)
                .NotEmpty().WithMessage("Tên team không được để trống");

            team.RuleFor(t => t)
                .Must(t => t.Player1Id != t.Player2Id)
                .WithMessage("Player1 và Player2 không được trùng nhau");
        });

        RuleFor(x => x.Teams)
            .Must(teams =>
            {
                var allPlayerIds = teams.SelectMany(t => new[] { t.Player1Id, t.Player2Id }).ToList();
                return allPlayerIds.Distinct().Count() == allPlayerIds.Count;
            })
            .WithMessage("Có người chơi xuất hiện trong nhiều teams");
    }
}
