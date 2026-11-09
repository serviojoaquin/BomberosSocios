using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Entities;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Categories.Commands.CreateCategory;

[Authorize(Permissions = Permissions.CreateCategories)]
public record CreateCategoryCommand : IRequest<int>
{
    public string? Description { get; init; }

    public decimal Amount { get; init; }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = new Category
        {
            Description = request.Description!,
            Amount = request.Amount
        };

        _context.Categories.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
