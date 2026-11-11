using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Cobradores.Queries.GetCobradorCuotasImpagas;

public class CobradorCuotasImpagasVm
{
    public CobradorResumenDto Cobrador { get; init; } = new();

    public IReadOnlyCollection<SocioCuotasPivotDto> Cuotas { get; init; } = [];
}

[Authorize(Permissions = Permissions.VerCobradores)]
public record GetCobradorCuotasImpagasQuery(int CobradorId, short Year) : IRequest<CobradorCuotasImpagasVm>;

public class GetCobradorCuotasImpagasQueryHandler : IRequestHandler<GetCobradorCuotasImpagasQuery, CobradorCuotasImpagasVm>
{
    private readonly IApplicationDbContext _context;
    private readonly ICobradorActual _cobradorActual;

    public GetCobradorCuotasImpagasQueryHandler(IApplicationDbContext context, ICobradorActual cobradorActual)
    {
        _context = context;
        _cobradorActual = cobradorActual;
    }

    public async Task<CobradorCuotasImpagasVm> Handle(GetCobradorCuotasImpagasQuery request, CancellationToken cancellationToken)
    {
        var propio = await _cobradorActual.GetCobradorIdAsync(cancellationToken);

        if (propio.HasValue && request.CobradorId != propio.Value)
        {
            Result.Failure(["Un cobrador solo puede consultar sus propias cuotas."]).ThrowIfFailed();
        }

        var cobrador = await _context.Cobradores
            .AsNoTracking()
            .Where(c => c.Id == request.CobradorId)
            .Select(c => new CobradorResumenDto { Id = c.Id, Nombre = c.Nombre, Apellido = c.Apellido, Comision = c.Comision ?? 0 })
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.CobradorId, cobrador);

        var cuotas = await _context.Cuotas
            .AsNoTracking()
            .Where(c => c.Estado == CuotaEstado.Impaga && c.Socio.CobradorId == request.CobradorId && c.Ano == request.Year)
            .Select(c => new { c.SocioId, SocioNombre = c.Socio.Nombre, c.Mes, c.Importe })
            .ToListAsync(cancellationToken);

        return new CobradorCuotasImpagasVm
        {
            Cobrador = cobrador,
            Cuotas = SocioCuotasPivotDto.Pivot(cuotas.Select(c => (c.SocioId, c.SocioNombre, (int)c.Mes, c.Importe)))
        };
    }
}
