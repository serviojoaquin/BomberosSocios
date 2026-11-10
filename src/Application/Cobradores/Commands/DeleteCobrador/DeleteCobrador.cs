using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Cobradores.Commands.DeleteCobrador;

[Authorize(Permissions = Permissions.EliminarCobradores)]
public record DeleteCobradorCommand(int Id) : IRequest;

public class DeleteCobradorCommandHandler : IRequestHandler<DeleteCobradorCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCobradorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCobradorCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Cobradores
            .Where(c => c.Id == request.Id)
            .SingleOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.Cobradores.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
