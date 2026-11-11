using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Application.Cuotas.Dtos;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Cuotas.Commands.BonificarCuotasViejas;

[Authorize(Permissions = Permissions.AdministrarCuotas)]
public record BonificarCuotasViejasCommand : IRequest<ChangeStatusCuotaResponse>
{
    public int? CobradorId { get; init; }

    public int? SocioId { get; init; }
}

public class BonificarCuotasViejasCommandHandler : IRequestHandler<BonificarCuotasViejasCommand, ChangeStatusCuotaResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICobradorActual _cobradorActual;
    private readonly TimeProvider _timeProvider;

    public BonificarCuotasViejasCommandHandler(IApplicationDbContext context, ICobradorActual cobradorActual, TimeProvider timeProvider)
    {
        _context = context;
        _cobradorActual = cobradorActual;
        _timeProvider = timeProvider;
    }

    public async Task<ChangeStatusCuotaResponse> Handle(BonificarCuotasViejasCommand request, CancellationToken cancellationToken)
    {
        var ahora = _timeProvider.GetLocalNow().DateTime;

        var cuotasQuery = _context.Cuotas
            .Include(c => c.Socio)
            .Where(c => c.Ano < ahora.Year && c.Estado == CuotaEstado.Impaga);

        if (request.SocioId.HasValue)
        {
            cuotasQuery = cuotasQuery.Where(c => c.SocioId == request.SocioId.Value);
        }
        else if (request.CobradorId.HasValue)
        {
            cuotasQuery = cuotasQuery.Where(c => c.Socio.CobradorId == request.CobradorId.Value);
        }

        var propio = await _cobradorActual.GetCobradorIdAsync(cancellationToken);

        if (propio.HasValue && await cuotasQuery.AnyAsync(c => c.Socio.CobradorId != propio.Value, cancellationToken))
        {
            Result.Failure(["Un cobrador solo puede bonificar cuotas de sus socios."]).ThrowIfFailed();
        }

        var cuotas = await cuotasQuery.ToListAsync(cancellationToken);

        foreach (var cuota in cuotas)
        {
            cuota.Bonificar(ahora, cuota.CobradorId);
        }

        if (cuotas.Count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new ChangeStatusCuotaResponse
        {
            CuotaEstadoId = (int)CuotaEstado.Bonificada,
            FechaBonificada = ahora,
            CuotasModifiedCount = cuotas.Count
        };
    }
}
