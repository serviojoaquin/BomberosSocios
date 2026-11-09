using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Categories.Commands.UpdateCategory;

[Authorize(Permissions = Permissions.EditCategories)]
public record UpdateCategoryCommand : IRequest
{
    public int Id { get; init; }

    public string? Description { get; init; }

    public decimal Amount { get; init; }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Categories
            .FindAsync([request.Id], cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Description = request.Description!;
        entity.Amount = request.Amount;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
