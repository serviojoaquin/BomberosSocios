namespace BaseApp.Application.Roles.Commands.CreateRole;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(50);
    }
}
