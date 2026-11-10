using BaseApp.Application.Common.Interfaces;

namespace BaseApp.Application.SampleItems.Commands.UpdateSampleItem;

public class UpdateSampleItemCommandValidator : AbstractValidator<UpdateSampleItemCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateSampleItemCommandValidator(IApplicationDbContext context)
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

    public async Task<bool> BeUniqueDescription(UpdateSampleItemCommand model, string? description, CancellationToken cancellationToken)
    {
        return !await _context.SampleItems
            .Where(c => c.Id != model.Id)
            .AnyAsync(c => c.Description == description, cancellationToken);
    }
}
