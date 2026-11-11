using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Entities;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Reportes.Commands.SaveCuotaSocio;

public class SaveCuotaSocioResponse
{
    public bool Success { get; init; }
}

[Authorize(Permissions = Permissions.VerCuotas)]
public record SaveCuotaSocioCommand : IRequest<SaveCuotaSocioResponse>
{
    public int SocioId { get; init; }

    public byte[] Reporte { get; init; } = [];

    public short Ano { get; init; }
}

public class SaveCuotaSocioCommandValidator : AbstractValidator<SaveCuotaSocioCommand>
{
    public SaveCuotaSocioCommandValidator()
    {
        RuleFor(v => v.Reporte).NotEmpty().WithMessage("El reporte está vacío.");
    }
}

public class SaveCuotaSocioCommandHandler : IRequestHandler<SaveCuotaSocioCommand, SaveCuotaSocioResponse>
{
    private readonly IApplicationDbContext _context;

    public SaveCuotaSocioCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SaveCuotaSocioResponse> Handle(SaveCuotaSocioCommand request, CancellationToken cancellationToken)
    {
        var existente = await _context.Reportes
            .FirstOrDefaultAsync(r => r.ReporteTipo == ReporteTipo.CuotasSocio && r.EntidadId == request.SocioId && r.Ano == request.Ano, cancellationToken);

        if (existente is not null)
        {
            existente.ReporteRaw = request.Reporte;
        }
        else
        {
            _context.Reportes.Add(new Reporte
            {
                ReporteRaw = request.Reporte,
                ReporteTipo = ReporteTipo.CuotasSocio,
                Ano = request.Ano,
                EntidadId = request.SocioId
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new SaveCuotaSocioResponse { Success = true };
    }
}
