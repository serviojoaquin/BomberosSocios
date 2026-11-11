using System.Globalization;
using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Cobradores.Queries.GetCobradorCuotasCobradas;

public class CobradorCuotasCobradasVm
{
    public CobradorResumenDto Cobrador { get; init; } = new();

    public IReadOnlyCollection<SocioCuotasPivotDto> Cuotas { get; init; } = [];

    public string? FechaDeCobroFromFormatted { get; init; }

    public string? FechaDeCobroToFormatted { get; init; }
}

[Authorize(Permissions = Permissions.VerCobradores)]
public record GetCobradorCuotasCobradasQuery(int CobradorId, short Year, DateTime? FechaDeCobroFrom, DateTime? FechaDeCobroTo) : IRequest<CobradorCuotasCobradasVm>;

public class GetCobradorCuotasCobradasQueryHandler : IRequestHandler<GetCobradorCuotasCobradasQuery, CobradorCuotasCobradasVm>
{
    private readonly IApplicationDbContext _context;
    private readonly ICobradorActual _cobradorActual;

    public GetCobradorCuotasCobradasQueryHandler(IApplicationDbContext context, ICobradorActual cobradorActual)
    {
        _context = context;
        _cobradorActual = cobradorActual;
    }

    public async Task<CobradorCuotasCobradasVm> Handle(GetCobradorCuotasCobradasQuery request, CancellationToken cancellationToken)
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

        var cuotasQuery = _context.Cuotas
            .AsNoTracking()
            .Where(c => c.Estado == CuotaEstado.Cobrada && c.CobradorId == request.CobradorId && c.Ano == request.Year);

        if (request.FechaDeCobroFrom.HasValue && request.FechaDeCobroTo.HasValue)
        {
            var desde = request.FechaDeCobroFrom.Value.Date;
            var hasta = request.FechaDeCobroTo.Value.Date.AddDays(1).AddSeconds(-1);

            cuotasQuery = cuotasQuery.Where(c => c.FechaDeCobro >= desde && c.FechaDeCobro <= hasta);
        }

        var cuotas = await cuotasQuery
            .Select(c => new { c.SocioId, SocioNombre = c.Socio.Nombre, c.Mes, c.Importe })
            .ToListAsync(cancellationToken);

        return new CobradorCuotasCobradasVm
        {
            Cobrador = cobrador,
            Cuotas = SocioCuotasPivotDto.Pivot(cuotas.Select(c => (c.SocioId, c.SocioNombre, (int)c.Mes, c.Importe))),
            FechaDeCobroFromFormatted = request.FechaDeCobroFrom?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            FechaDeCobroToFormatted = request.FechaDeCobroTo?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
        };
    }
}
