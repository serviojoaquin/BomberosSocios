using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Entities;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Cuotas.Queries.GetCuotasImprimir;

public class CuotaImprimirDto
{
    public int Id { get; init; }

    public decimal Importe { get; init; }

    public short Ano { get; init; }

    public int Mes { get; init; }

    public string MesDescripcion { get; init; } = string.Empty;

    public int CuotaEstadoId { get; init; }

    public string CuotaEstado { get; init; } = string.Empty;
}

public class SocioImprimirDto
{
    public int Id { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public string Domicilio { get; init; } = string.Empty;

    public string? DomicilioDeCobro { get; init; }

    public string? Telefono { get; init; }

    public DateTime FechaAfiliacion { get; init; }

    public string Cobrador { get; init; } = string.Empty;

    public IReadOnlyCollection<CuotaImprimirDto> Cuotas { get; init; } = [];
}

public class CuotasImprimirVm
{
    public IReadOnlyCollection<SocioImprimirDto> Socios { get; init; } = [];
}

[Authorize(Permissions = Permissions.VerCuotas)]
public record GetCuotasImprimirQuery(int? CobradorId, int? SocioId, int Year, bool Regenerar) : IRequest<CuotasImprimirVm>;

public class GetCuotasImprimirQueryHandler : IRequestHandler<GetCuotasImprimirQuery, CuotasImprimirVm>
{
    private const int CantMaxSocios = 50;

    private readonly IApplicationDbContext _context;
    private readonly ICobradorActual _cobradorActual;

    public GetCuotasImprimirQueryHandler(IApplicationDbContext context, ICobradorActual cobradorActual)
    {
        _context = context;
        _cobradorActual = cobradorActual;
    }

    public async Task<CuotasImprimirVm> Handle(GetCuotasImprimirQuery request, CancellationToken cancellationToken)
    {
        var propio = await _cobradorActual.GetCobradorIdAsync(cancellationToken);

        if (propio.HasValue && request.CobradorId.HasValue && request.CobradorId != propio.Value)
        {
            Result.Failure(["Un cobrador solo puede imprimir cuotas de sus socios."]).ThrowIfFailed();
        }

        var cuotasQuery = _context.Cuotas
            .AsNoTracking()
            .Where(c => c.Ano == request.Year && c.Socio.Estado == SocioEstado.Activo);

        if (request.SocioId.HasValue)
        {
            cuotasQuery = cuotasQuery.Where(c => c.SocioId == request.SocioId.Value);
        }

        if (request.CobradorId.HasValue)
        {
            cuotasQuery = cuotasQuery.Where(c => c.Socio.CobradorId == request.CobradorId.Value);
        }

        if (propio.HasValue)
        {
            cuotasQuery = cuotasQuery.Where(c => c.Socio.CobradorId == propio.Value);
        }

        var cuotas = await cuotasQuery
            .Include(c => c.Socio).ThenInclude(s => s.Cobrador)
            .ToListAsync(cancellationToken);

        var sociosConReporte = await _context.Reportes
            .Where(r => r.Ano == request.Year && r.ReporteTipo == ReporteTipo.CuotasSocio)
            .Select(r => r.EntidadId)
            .ToListAsync(cancellationToken);

        var socios = cuotas
            .Select(c => c.Socio)
            .DistinctBy(s => s.Id)
            .Where(s => request.Regenerar || !sociosConReporte.Contains(s.Id))
            .OrderBy(s => s.Id)
            .Take(CantMaxSocios)
            .Select(s => new SocioImprimirDto
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Domicilio = s.Domicilio,
                DomicilioDeCobro = s.DomicilioDeCobro,
                Telefono = s.Telefono,
                FechaAfiliacion = s.FechaAfiliacion,
                Cobrador = s.Cobrador.NombreCompleto,
                Cuotas = Enumerable.Range(1, 12)
                    .Select(mes => cuotas.FirstOrDefault(c => c.SocioId == s.Id && (int)c.Mes == mes) ?? new Cuota
                    {
                        Mes = (Mes)mes,
                        Ano = (short)request.Year,
                        Estado = CuotaEstado.Invalida
                    })
                    .OrderByDescending(c => c.Mes)
                    .Select(c => new CuotaImprimirDto
                    {
                        Id = c.Id,
                        Importe = c.Importe,
                        Ano = c.Ano,
                        Mes = (int)c.Mes,
                        MesDescripcion = c.Mes.ToString(),
                        CuotaEstadoId = (int)c.Estado,
                        CuotaEstado = c.Estado.ToString()
                    })
                    .ToList()
            })
            .ToList();

        return new CuotasImprimirVm { Socios = socios };
    }
}
