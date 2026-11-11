using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Socios.Commands.DeleteSocio;

[Authorize(Permissions = Permissions.EliminarSocios)]
public record DeleteSocioCommand(int Id) : IRequest;

public class DeleteSocioCommandHandler : IRequestHandler<DeleteSocioCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICobradorActual _cobradorActual;

    public DeleteSocioCommandHandler(IApplicationDbContext context, ICobradorActual cobradorActual)
    {
        _context = context;
        _cobradorActual = cobradorActual;
    }

    public async Task Handle(DeleteSocioCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Socios
            .Where(s => s.Id == request.Id)
            .SingleOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        var propio = await _cobradorActual.GetCobradorIdAsync(cancellationToken);

        if (propio.HasValue && entity.CobradorId != propio.Value)
        {
            Result.Failure(["El socio pertenece a otro cobrador."]).ThrowIfFailed();
        }

        _context.Socios.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
