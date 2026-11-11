using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Cuotas.Dtos;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Cuotas.Commands.CobrarCuotasQr;

public record CobrarCuotasQrCommand : IRequest<ChangeStatusCuotaResponse>
{
    public IReadOnlyCollection<int> CuotasIds { get; init; } = [];
}

public class CobrarCuotasQrCommandHandler : IRequestHandler<CobrarCuotasQrCommand, ChangeStatusCuotaResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public CobrarCuotasQrCommandHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<ChangeStatusCuotaResponse> Handle(CobrarCuotasQrCommand request, CancellationToken cancellationToken)
    {
        var cuotas = await _context.Cuotas
            .Include(c => c.Socio)
            .Where(c => request.CuotasIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        var ahora = _timeProvider.GetLocalNow().DateTime;

        foreach (var cuota in cuotas)
        {
            cuota.CobrarPorQr(ahora, cuota.Socio.CobradorId);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ChangeStatusCuotaResponse
        {
            CuotaEstadoId = (int)CuotaEstado.CobradaQr,
            FechaDeCobro = ahora,
            CuotasModifiedCount = cuotas.Count
        };
    }
}
