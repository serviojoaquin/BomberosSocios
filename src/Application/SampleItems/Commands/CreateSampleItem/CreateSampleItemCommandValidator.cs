using BaseApp.Application.Common.Interfaces;

namespace BaseApp.Application.SampleItems.Commands.CreateSampleItem;

public class CreateSampleItemCommandValidator : AbstractValidator<CreateSampleItemCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateSampleItemCommandValidator(IApplicationDbContext context)
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

    public async Task<bool> BeUniqueDescription(string? description, CancellationToken cancellationToken)
    {
        return !await _context.SampleItems
            .AnyAsync(c => c.Description == description, cancellationToken);
    }
}
