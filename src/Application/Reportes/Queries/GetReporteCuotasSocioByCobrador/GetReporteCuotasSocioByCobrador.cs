using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Application.Reportes.Queries.GetReporteCuotasSocio;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Reportes.Queries.GetReporteCuotasSocioByCobrador;

[Authorize(Permissions = Permissions.VerCuotas)]
public record GetReporteCuotasSocioByCobradorQuery(int CobradorId, int Year) : IRequest<ReporteArchivoVm>;

public class GetReporteCuotasSocioByCobradorQueryHandler : IRequestHandler<GetReporteCuotasSocioByCobradorQuery, ReporteArchivoVm>
{
    private readonly IApplicationDbContext _context;
    private readonly IPdfFileBuilder _pdfFileBuilder;
    private readonly ICobradorActual _cobradorActual;

    public GetReporteCuotasSocioByCobradorQueryHandler(IApplicationDbContext context, IPdfFileBuilder pdfFileBuilder, ICobradorActual cobradorActual)
    {
        _context = context;
        _pdfFileBuilder = pdfFileBuilder;
        _cobradorActual = cobradorActual;
    }

    public async Task<ReporteArchivoVm> Handle(GetReporteCuotasSocioByCobradorQuery request, CancellationToken cancellationToken)
    {
        var propio = await _cobradorActual.GetCobradorIdAsync(cancellationToken);

        if (propio.HasValue && request.CobradorId != propio.Value)
        {
            Result.Failure(["Un cobrador solo puede descargar los reportes de sus socios."]).ThrowIfFailed();
        }

        var sociosIds = _context.Socios
            .Where(s => s.CobradorId == request.CobradorId && s.Estado == SocioEstado.Activo)
            .Select(s => s.Id);

        var reportes = await _context.Reportes
            .AsNoTracking()
            .Where(r => r.ReporteTipo == ReporteTipo.CuotasSocio && r.Ano == request.Year && sociosIds.Contains(r.EntidadId))
            .OrderBy(r => r.EntidadId)
            .Select(r => r.ReporteRaw)
            .ToListAsync(cancellationToken);

        if (reportes.Count == 0)
        {
            return new ReporteArchivoVm();
        }

        return new ReporteArchivoVm
        {
            Content = _pdfFileBuilder.Merge(reportes),
            FileName = $"CuotasSocioByCobrador_{request.CobradorId}_Ano_{request.Year}.pdf"
        };
    }
}
