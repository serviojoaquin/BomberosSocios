using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Cuotas.Queries.GetCuotasSocio;

public class CuotaDto
{
    public int Id { get; init; }

    public short Ano { get; init; }

    public int Mes { get; init; }

    public string MesNombre { get; init; } = string.Empty;

    public decimal Importe { get; init; }

    public int Estado { get; init; }

    public string EstadoNombre { get; init; } = string.Empty;

    public DateTime? FechaDeCobro { get; init; }

    public DateTime? FechaBonificada { get; init; }

    public string? Cobrador { get; init; }
}

public class CuotasSocioVm
{
    public int SocioId { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public string Categoria { get; init; } = string.Empty;

    public string Cobrador { get; init; } = string.Empty;

    public bool Activo { get; init; }

    public IReadOnlyCollection<CuotaDto> Cuotas { get; init; } = [];
}

[Authorize(Permissions = Permissions.VerCuotas)]
public record GetCuotasSocioQuery(int SocioId) : IRequest<CuotasSocioVm>;

public class GetCuotasSocioQueryHandler : IRequestHandler<GetCuotasSocioQuery, CuotasSocioVm>
{
    private readonly IApplicationDbContext _context;
    private readonly ICobradorActual _cobradorActual;

    public GetCuotasSocioQueryHandler(IApplicationDbContext context, ICobradorActual cobradorActual)
    {
        _context = context;
        _cobradorActual = cobradorActual;
    }

    public async Task<CuotasSocioVm> Handle(GetCuotasSocioQuery request, CancellationToken cancellationToken)
    {
        var socio = await _context.Socios
            .AsNoTracking()
            .Include(s => s.Categoria)
            .Include(s => s.Cobrador)
            .FirstOrDefaultAsync(s => s.Id == request.SocioId, cancellationToken);

        Guard.Against.NotFound(request.SocioId, socio);

        var propio = await _cobradorActual.GetCobradorIdAsync(cancellationToken);

        if (propio.HasValue && socio.CobradorId != propio.Value)
        {
            Result.Failure(["El socio pertenece a otro cobrador."]).ThrowIfFailed();
        }

        var cuotas = await _context.Cuotas
            .AsNoTracking()
            .Include(c => c.Cobrador)
            .Where(c => c.SocioId == request.SocioId)
            .OrderByDescending(c => c.Ano).ThenBy(c => c.Mes)
            .ToListAsync(cancellationToken);

        return new CuotasSocioVm
        {
            SocioId = socio.Id,
            Nombre = socio.Nombre,
            Categoria = socio.Categoria.Descripcion,
            Cobrador = socio.Cobrador.NombreCompleto,
            Activo = socio.Estado == SocioEstado.Activo,
            Cuotas = cuotas.Select(c => new CuotaDto
            {
                Id = c.Id,
                Ano = c.Ano,
                Mes = (int)c.Mes,
                MesNombre = c.Mes.ToString(),
                Importe = c.Importe,
                Estado = (int)c.Estado,
                EstadoNombre = CuotaRules.Descripcion(c.Estado),
                FechaDeCobro = c.FechaDeCobro,
                FechaBonificada = c.FechaBonificada,
                Cobrador = c.Cobrador?.NombreCompleto
            }).ToList()
        };
    }
}
