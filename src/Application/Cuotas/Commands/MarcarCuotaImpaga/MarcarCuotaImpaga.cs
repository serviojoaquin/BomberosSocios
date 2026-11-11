using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Application.Cuotas.Dtos;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Cuotas.Commands.MarcarCuotaImpaga;

[Authorize(Permissions = Permissions.PagarCuotas)]
public record MarcarCuotaImpagaCommand(int CuotaId) : IRequest<ChangeStatusCuotaResponse>;

public class MarcarCuotaImpagaCommandHandler : IRequestHandler<MarcarCuotaImpagaCommand, ChangeStatusCuotaResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICobradorActual _cobradorActual;

    public MarcarCuotaImpagaCommandHandler(IApplicationDbContext context, ICobradorActual cobradorActual)
    {
        _context = context;
        _cobradorActual = cobradorActual;
    }

    public async Task<ChangeStatusCuotaResponse> Handle(MarcarCuotaImpagaCommand request, CancellationToken cancellationToken)
    {
        var cuotas = await CuotaRules.CargarCuotasPropiasAsync(_context, _cobradorActual, [request.CuotaId], cancellationToken);

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
