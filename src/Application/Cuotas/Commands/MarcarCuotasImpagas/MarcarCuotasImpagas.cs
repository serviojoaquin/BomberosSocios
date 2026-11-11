using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Cuotas.Dtos;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Cuotas.Commands.MarcarCuotasImpagas;

public record MarcarCuotasImpagasCommand : IRequest<ChangeStatusCuotaResponse>
{
    public IReadOnlyCollection<int> CuotasIds { get; init; } = [];
}

public class MarcarCuotasImpagasCommandHandler : IRequestHandler<MarcarCuotasImpagasCommand, ChangeStatusCuotaResponse>
{
    private readonly IApplicationDbContext _context;

    public MarcarCuotasImpagasCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChangeStatusCuotaResponse> Handle(MarcarCuotasImpagasCommand request, CancellationToken cancellationToken)
    {
        var cuotas = await _context.Cuotas
            .Where(c => request.CuotasIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        foreach (var cuota in cuotas)
        {
            cuota.MarcarImpaga();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ChangeStatusCuotaResponse
        {
            CuotaEstadoId = (int)CuotaEstado.Impaga,
            CuotasModifiedCount = cuotas.Count
        };
    }
}
