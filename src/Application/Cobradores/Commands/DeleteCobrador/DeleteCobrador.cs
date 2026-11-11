using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;
using BaseApp.Domain.Events;

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

        if (await _context.Socios.AnyAsync(s => s.CobradorId == entity.Id, cancellationToken))
        {
            Result.Failure(["El cobrador tiene socios asignados y no se puede eliminar."]).ThrowIfFailed();
        }

        if (await _context.Cuotas.AnyAsync(c => c.CobradorId == entity.Id, cancellationToken))
        {
            Result.Failure(["El cobrador tiene cuotas cobradas y no se puede eliminar."]).ThrowIfFailed();
        }

        _context.Cobradores.Remove(entity);
        entity.AddDomainEvent(new EliminarCajaEvent(entity.Id));

        await _context.SaveChangesAsync(cancellationToken);
    }
}
