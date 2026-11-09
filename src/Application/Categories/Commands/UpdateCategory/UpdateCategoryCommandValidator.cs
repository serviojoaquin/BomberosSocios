using BaseApp.Application.Common.Interfaces;

namespace BaseApp.Application.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Description)
            .NotEmpty()
            .MaximumLength(200)
            .MustAsync(BeUniqueDescription)
                .WithMessage("'{PropertyName}' must be unique.")
                .WithErrorCode("Unique");

        RuleFor(v => v.Amount)
            .GreaterThanOrEqualTo(0);
    }

    public async Task<bool> BeUniqueDescription(UpdateCategoryCommand model, string? description, CancellationToken cancellationToken)
    {
        return !await _context.Categories
            .Where(c => c.Id != model.Id)
            .AnyAsync(c => c.Description == description, cancellationToken);
    }
}
