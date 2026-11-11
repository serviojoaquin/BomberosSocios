using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Cuotas.Dtos;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Cuotas.Commands.MarcarCuotasQrCreado;

public record MarcarCuotasQrCreadoCommand : IRequest<ChangeStatusCuotaResponse>
{
    public IReadOnlyCollection<int> CuotasIds { get; init; } = [];
}

public class MarcarCuotasQrCreadoCommandHandler : IRequestHandler<MarcarCuotasQrCreadoCommand, ChangeStatusCuotaResponse>
{
    private readonly IApplicationDbContext _context;

    public MarcarCuotasQrCreadoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChangeStatusCuotaResponse> Handle(MarcarCuotasQrCreadoCommand request, CancellationToken cancellationToken)
    {
        var cuotas = await _context.Cuotas
            .Where(c => request.CuotasIds.Contains(c.Id) && c.Estado == CuotaEstado.Impaga)
            .ToListAsync(cancellationToken);

        foreach (var cuota in cuotas)
        {
            cuota.MarcarQrCreado();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ChangeStatusCuotaResponse
        {
            CuotaEstadoId = (int)CuotaEstado.QrCreado,
            CuotasModifiedCount = cuotas.Count
        };
    }
}
