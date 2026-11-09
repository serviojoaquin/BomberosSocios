using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Categories.Commands.DeleteCategory;

[Authorize(Permissions = Permissions.DeleteCategories)]
public record DeleteCategoryCommand(int Id) : IRequest;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Categories
            .Where(c => c.Id == request.Id)
            .SingleOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.Categories.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
