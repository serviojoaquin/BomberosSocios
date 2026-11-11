using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Dashboard;

public class CobranzaCuotaSociosDto
{
    public const int Buckets = 13;

    public int? CobradorId { get; init; }

    public string? CobradorNombre { get; init; }

    public string? CobradorApellido { get; init; }

    public int[] Cant { get; init; } = new int[Buckets];

    public decimal[] PorcI { get; set; } = new decimal[Buckets];

    public decimal[] PorcG { get; set; } = new decimal[Buckets];

    public int Total { get; init; }

    public decimal PorcGT { get; set; }
}

public class CobranzaCuotaSociosVm
{
    public IReadOnlyCollection<CobranzaCuotaSociosDto> CobranzaCuotaSociosList { get; init; } = [];
}

[Authorize(Permissions = Permissions.VerEstadisticas)]
public record CobranzaCuotaSociosQuery(int Year) : IRequest<CobranzaCuotaSociosVm>;

public class CobranzaCuotaSociosQueryHandler : IRequestHandler<CobranzaCuotaSociosQuery, CobranzaCuotaSociosVm>
{
    private readonly IApplicationDbContext _context;
    private readonly ICobradorActual _cobradorActual;

    public CobranzaCuotaSociosQueryHandler(IApplicationDbContext context, ICobradorActual cobradorActual)
    {
        _context = context;
        _cobradorActual = cobradorActual;
    }

    public async Task<CobranzaCuotaSociosVm> Handle(CobranzaCuotaSociosQuery request, CancellationToken cancellationToken)
    {
        var cobradas = await _context.Cuotas
            .AsNoTracking()
            .Where(c => c.Socio.Estado == SocioEstado.Activo && c.Estado == CuotaEstado.Cobrada && c.Ano == request.Year && c.CobradorId != null)
            .GroupBy(c => new { c.SocioId, CobradorId = c.CobradorId!.Value })
            .Select(g => new { g.Key.CobradorId, CantCuotasPagas = g.Count() })
            .ToListAsync(cancellationToken);

        var sociosConCobradas = await _context.Cuotas
            .AsNoTracking()
            .Where(c => c.Estado == CuotaEstado.Cobrada)
            .Select(c => c.SocioId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var sinCobros = await _context.Socios
            .AsNoTracking()
            .Where(s => s.Estado == SocioEstado.Activo
                && s.Cuotas.Any(c => c.Estado == CuotaEstado.Impaga && c.Ano == request.Year)
                && !sociosConCobradas.Contains(s.Id))
            .Select(s => s.CobradorId)
            .ToListAsync(cancellationToken);

        var sinCuotas = await _context.Socios
            .AsNoTracking()
            .Where(s => s.Estado == SocioEstado.Activo && !s.Cuotas.Any())
            .Select(s => s.CobradorId)
            .ToListAsync(cancellationToken);

        var celdas = cobradas
            .Select(c => (c.CobradorId, Bucket: Math.Min(c.CantCuotasPagas, CobranzaCuotaSociosDto.Buckets - 1)))
            .Concat(sinCobros.Select(id => (CobradorId: id, Bucket: 0)))
            .Concat(sinCuotas.Select(id => (CobradorId: id, Bucket: 0)))
            .ToList();

        if (celdas.Count == 0)
        {
            return new CobranzaCuotaSociosVm();
        }

        var cobradores = await _context.Cobradores
            .AsNoTracking()
            .Select(c => new { c.Id, c.Nombre, c.Apellido, SociosCount = c.Socios.Count })
            .ToListAsync(cancellationToken);

        var filas = celdas
            .GroupBy(c => c.CobradorId)
            .OrderByDescending(g => g.Key)
            .Select(g =>
            {
                var cobrador = cobradores.FirstOrDefault(c => c.Id == g.Key);
                var cant = new int[CobranzaCuotaSociosDto.Buckets];

                foreach (var celda in g)
                {
                    cant[celda.Bucket]++;
                }

                return new CobranzaCuotaSociosDto
                {
                    CobradorId = g.Key,
                    CobradorNombre = cobrador?.Nombre,
                    CobradorApellido = cobrador?.Apellido,
                    Cant = cant,
                    Total = cant.Sum()
                };
            })
            .ToList();

        var propio = await _cobradorActual.GetCobradorIdAsync(cancellationToken);

        if (propio.HasValue)
        {
            filas = filas.Where(f => f.CobradorId == propio.Value).ToList();
        }

        var totales = new int[CobranzaCuotaSociosDto.Buckets];

        foreach (var fila in filas)
        {
            for (var i = 0; i < CobranzaCuotaSociosDto.Buckets; i++)
            {
                totales[i] += fila.Cant[i];
            }
        }

        filas.Add(new CobranzaCuotaSociosDto { Cant = totales, Total = totales.Sum() });

        var sociosActivosCount = await _context.Socios.CountAsync(s => s.Estado == SocioEstado.Activo, cancellationToken);

        if (sociosActivosCount > 0)
        {
            foreach (var fila in filas)
            {
                var sociosDelCobrador = fila.CobradorId.HasValue ? cobradores.FirstOrDefault(c => c.Id == fila.CobradorId)?.SociosCount ?? 0 : 0;

                for (var i = 0; i < CobranzaCuotaSociosDto.Buckets; i++)
                {
                    fila.PorcG[i] = (decimal)fila.Cant[i] / sociosActivosCount;
                    fila.PorcI[i] = fila.CobradorId.HasValue
                        ? (sociosDelCobrador > 0 ? (decimal)fila.Cant[i] / sociosDelCobrador : -1)
                        : 0;
                }

                fila.PorcGT = (decimal)fila.Total / sociosActivosCount;
            }
        }

        return new CobranzaCuotaSociosVm { CobranzaCuotaSociosList = filas };
    }
}
