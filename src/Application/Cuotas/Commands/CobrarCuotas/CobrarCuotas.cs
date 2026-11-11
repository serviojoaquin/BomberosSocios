using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Application.Cuotas.Dtos;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Cuotas.Commands.CobrarCuotas;

[Authorize(Permissions = Permissions.PagarCuotas)]
public record CobrarCuotasCommand : IRequest<ChangeStatusCuotaResponse>
{
    public IReadOnlyCollection<int> CuotasIds { get; init; } = [];
}

public class CobrarCuotasCommandValidator : AbstractValidator<CobrarCuotasCommand>
{
    public CobrarCuotasCommandValidator()
    {
        RuleFor(v => v.CuotasIds)
            .NotEmpty().WithMessage("Seleccioná al menos una cuota.");
    }
}

public class CobrarCuotasCommandHandler : IRequestHandler<CobrarCuotasCommand, ChangeStatusCuotaResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICobradorActual _cobradorActual;
    private readonly TimeProvider _timeProvider;

    public CobrarCuotasCommandHandler(IApplicationDbContext context, ICobradorActual cobradorActual, TimeProvider timeProvider)
    {
        _context = context;
        _cobradorActual = cobradorActual;
        _timeProvider = timeProvider;
    }

    public async Task<ChangeStatusCuotaResponse> Handle(CobrarCuotasCommand request, CancellationToken cancellationToken)
    {
        var cuotas = await CuotaRules.CargarCuotasPropiasAsync(_context, _cobradorActual, request.CuotasIds, cancellationToken);
        var ahora = _timeProvider.GetLocalNow().DateTime;

        foreach (var cuota in cuotas)
        {
            cuota.Cobrar(ahora, cuota.Socio.CobradorId);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ChangeStatusCuotaResponse
        {
            CuotaEstadoId = (int)CuotaEstado.Cobrada,
            FechaDeCobro = ahora,
            CuotasModifiedCount = cuotas.Count
        };
    }
}
