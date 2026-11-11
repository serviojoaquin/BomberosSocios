using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Entities;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Cuotas.Commands.GenerarCuotas;

[Authorize(Permissions = Permissions.AdministrarCuotas)]
public record GenerarCuotasCommand : IRequest<int>
{
    public short Year { get; init; }

    public int? CobradorId { get; init; }

    public int? SocioId { get; init; }

    public bool ForceRecreate { get; init; }

    public byte? Parte { get; init; }
}

public class GenerarCuotasCommandValidator : AbstractValidator<GenerarCuotasCommand>
{
    public GenerarCuotasCommandValidator()
    {
        RuleFor(v => v.Year)
            .InclusiveBetween((short)2000, (short)2100).WithMessage("El año no es válido.");
    }
}

public class GenerarCuotasCommandHandler : IRequestHandler<GenerarCuotasCommand, int>
{
    private const int SociosPorParte = 200;

    private readonly IApplicationDbContext _context;
    private readonly ICobradorActual _cobradorActual;
    private readonly IReportService _reportService;

    public GenerarCuotasCommandHandler(IApplicationDbContext context, ICobradorActual cobradorActual, IReportService reportService)
    {
        _context = context;
        _cobradorActual = cobradorActual;
        _reportService = reportService;
    }

    public async Task<int> Handle(GenerarCuotasCommand request, CancellationToken cancellationToken)
    {
        var sociosQuery = _context.Socios.Where(s => s.Estado == SocioEstado.Activo);

        if (request.SocioId.HasValue)
        {
            sociosQuery = sociosQuery.Where(s => s.Id == request.SocioId.Value);
        }
        else if (request.CobradorId.HasValue)
        {
            sociosQuery = sociosQuery.Where(s => s.CobradorId == request.CobradorId.Value);
        }

        if (request.Parte.HasValue)
        {
            sociosQuery = sociosQuery
                .OrderBy(s => s.Id)
                .Skip((request.Parte.Value - 1) * SociosPorParte)
                .Take(SociosPorParte);
        }

        sociosQuery = request.ForceRecreate
            ? sociosQuery.Where(s => s.Cuotas.Any(c => c.Ano == request.Year))
            : sociosQuery.Where(s => !s.Cuotas.Any(c => c.Ano == request.Year));

        var socios = await sociosQuery
            .Include(s => s.Categoria)
            .Include(s => s.Cobrador)
            .ToListAsync(cancellationToken);

        if (socios.Count == 0)
        {
            return 0;
        }

        var propio = await _cobradorActual.GetCobradorIdAsync(cancellationToken);

        if (propio.HasValue && socios.Any(s => s.CobradorId != propio.Value))
        {
            Result.Failure(["Un cobrador solo puede generar cuotas de sus socios."]).ThrowIfFailed();
        }

        var sociosIds = socios.Select(s => s.Id).ToList();

        var existentes = await _context.Cuotas
            .Include(c => c.Cobrador)
            .Where(c => c.Ano >= request.Year && sociosIds.Contains(c.SocioId))
            .ToListAsync(cancellationToken);

        if (request.ForceRecreate)
        {
            var impagas = existentes.Where(c => c.Estado == CuotaEstado.Impaga).ToList();

            _context.Cuotas.RemoveRange(impagas);
            existentes = existentes.Except(impagas).ToList();
        }

        var pagadas = existentes
            .Where(c => c.Ano == request.Year && c.Estado != CuotaEstado.Impaga)
            .ToList();

        var creadas = new List<Cuota>();

        foreach (var socio in socios)
        {
            var mesInicial = socio.FechaAfiliacion.Year == request.Year ? socio.FechaAfiliacion.Month : 1;

            for (var mes = mesInicial; mes <= 12; mes++)
            {
                if (pagadas.Any(c => c.SocioId == socio.Id && (int)c.Mes == mes))
                {
                    continue;
                }

                var cuota = new Cuota
                {
                    SocioId = socio.Id,
                    Socio = socio,
                    Ano = request.Year,
                    Mes = (Mes)mes,
                    Importe = socio.Categoria.ImporteDelMes(mes),
                    Estado = CuotaEstado.Impaga
                };

                _context.Cuotas.Add(cuota);
                creadas.Add(cuota);
            }
        }

        var sociosConCuotas = creadas.Select(c => c.Socio).Distinct().ToList();
        var sociosConCuotasIds = sociosConCuotas.Select(s => s.Id).ToList();

        var reportesViejos = await _context.Reportes
            .Where(r => r.Ano == request.Year && r.ReporteTipo == ReporteTipo.CuotasSocio && sociosConCuotasIds.Contains(r.EntidadId))
            .ToListAsync(cancellationToken);

        _context.Reportes.RemoveRange(reportesViejos);

        foreach (var socio in sociosConCuotas)
        {
            var cuotasDelAno = pagadas.Where(c => c.SocioId == socio.Id)
                .Concat(creadas.Where(c => c.SocioId == socio.Id))
                .ToList();

            _context.Reportes.Add(new Reporte
            {
                ReporteRaw = _reportService.GeneratePdfReportCuotasSocio(socio, cuotasDelAno, request.Year),
                ReporteTipo = ReporteTipo.CuotasSocio,
                Ano = request.Year,
                EntidadId = socio.Id
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return creadas.Count;
    }
}
