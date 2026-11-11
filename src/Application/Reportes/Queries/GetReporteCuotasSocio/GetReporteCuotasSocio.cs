using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Reportes.Queries.GetReporteCuotasSocio;

public class ReporteArchivoVm
{
    public string FileName { get; init; } = "ArchivoVacio.pdf";

    public string ContentType { get; init; } = "application/pdf";

    public byte[] Content { get; init; } = [];
}

[Authorize(Permissions = Permissions.VerCuotas)]
public record GetReporteCuotasSocioQuery(int SocioId, int Year) : IRequest<ReporteArchivoVm>;

public class GetReporteCuotasSocioQueryHandler : IRequestHandler<GetReporteCuotasSocioQuery, ReporteArchivoVm>
{
    private readonly IApplicationDbContext _context;
    private readonly ICobradorActual _cobradorActual;

    public GetReporteCuotasSocioQueryHandler(IApplicationDbContext context, ICobradorActual cobradorActual)
    {
        _context = context;
        _cobradorActual = cobradorActual;
    }

    public async Task<ReporteArchivoVm> Handle(GetReporteCuotasSocioQuery request, CancellationToken cancellationToken)
    {
        var propio = await _cobradorActual.GetCobradorIdAsync(cancellationToken);

        if (propio.HasValue)
        {
            var cobradorDelSocio = await _context.Socios
                .Where(s => s.Id == request.SocioId)
                .Select(s => (int?)s.CobradorId)
                .FirstOrDefaultAsync(cancellationToken);

            if (cobradorDelSocio != propio.Value)
            {
                Result.Failure(["El socio pertenece a otro cobrador."]).ThrowIfFailed();
            }
        }

        var reporte = await _context.Reportes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.EntidadId == request.SocioId && r.Ano == request.Year && r.ReporteTipo == ReporteTipo.CuotasSocio, cancellationToken);

        if (reporte is null)
        {
            return new ReporteArchivoVm();
        }

        return new ReporteArchivoVm
        {
            Content = reporte.ReporteRaw,
            FileName = $"CuotasSocio_{request.SocioId}_Ano_{request.Year}.pdf"
        };
    }
}
