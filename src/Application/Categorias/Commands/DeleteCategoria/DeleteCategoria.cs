using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Categorias.Commands.DeleteCategoria;

[Authorize(Permissions = Permissions.EliminarCategorias)]
public record DeleteCategoriaCommand(int Id) : IRequest;

public class DeleteCategoriaCommandHandler : IRequestHandler<DeleteCategoriaCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoriaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCategoriaCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Categorias
            .Where(c => c.Id == request.Id)
            .SingleOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.Categorias.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
