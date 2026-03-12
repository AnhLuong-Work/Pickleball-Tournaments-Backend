using FluentValidation;

namespace AppPickleball.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        When(x => x.Name != null, () => {
            RuleFor(x => x.Name).MinimumLength(2).MaximumLength(100);
        });
        When(x => x.Bio != null, () => {
            RuleFor(x => x.Bio).MaximumLength(500);
        });
        When(x => x.SkillLevel.HasValue, () => {
            RuleFor(x => x.SkillLevel!.Value).InclusiveBetween(1.0m, 5.0m);
        });
        When(x => x.DominantHand != null, () => {
            RuleFor(x => x.DominantHand).Must(h => h == "left" || h == "right")
                .WithMessage("Tay thuận phải là 'left' hoặc 'right'");
        });
        When(x => x.PaddleType != null, () => {
            RuleFor(x => x.PaddleType).MaximumLength(100);
        });
    }
}
