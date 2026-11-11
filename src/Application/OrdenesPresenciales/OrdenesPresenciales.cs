using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Application.Cuotas.Commands.CobrarCuotasQr;
using BaseApp.Application.Cuotas.Commands.MarcarCuotasImpagas;
using BaseApp.Application.Cuotas.Commands.MarcarCuotasQrCreado;
using BaseApp.Domain.Enums;
using MercadoPagoExtension.Contracts;
using MercadoPagoExtension.Contracts.Orden;
using MercadoPagoExtension.Entities;
using Microsoft.Extensions.Logging;
using SociosApp.MercadoPagoExtension.Enums;
using SociosApp.MercadoPagoExtension.Services.Interfaces;

namespace BaseApp.Application.OrdenesPresenciales;

[Authorize(Permissions = Permissions.PagosMercadoPago)]
public record CrearOrdenCommand : IRequest
{
    public IReadOnlyCollection<int> CuotasIds { get; init; } = [];
}

public class CrearOrdenCommandHandler : IRequestHandler<CrearOrdenCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICobradorActual _cobradorActual;
    private readonly IMercadoPagoService _mercadoPagoService;
    private readonly IMpConfigurationManager _mpConfigurationManager;
    private readonly TimeProvider _timeProvider;

    public CrearOrdenCommandHandler(IApplicationDbContext context, ICobradorActual cobradorActual, IMercadoPagoService mercadoPagoService, IMpConfigurationManager mpConfigurationManager, TimeProvider timeProvider)
    {
        _context = context;
        _cobradorActual = cobradorActual;
        _mercadoPagoService = mercadoPagoService;
        _mpConfigurationManager = mpConfigurationManager;
        _timeProvider = timeProvider;
    }

    public async Task Handle(CrearOrdenCommand request, CancellationToken cancellationToken)
    {
        var cuotas = await Cuotas.CuotaRules.CargarCuotasPropiasAsync(_context, _cobradorActual, request.CuotasIds, cancellationToken);
        var propio = await _cobradorActual.GetCobradorIdAsync(cancellationToken);
        var externalPosId = propio.HasValue ? $"CajaCobrador{propio.Value}" : "CajaPrincipal";

        var primera = cuotas.FirstOrDefault();

        if (primera is null)
        {
            return;
        }

        var crearOrdenRequest = new CrearOrdenRequest
        {
            Title = "Pago de cuotas con QR",
            Description = $"Pago de las cuotas ({string.Join(" - ", cuotas.Select(c => (int)c.Mes))}) del año {primera.Ano} del socio {primera.SocioId}.",
            ExpirationDate = _timeProvider.GetLocalNow().AddMinutes(2).ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz"),
            NotificationUrl = _mpConfigurationManager.NotificationUrl,
            TotalAmount = cuotas.Sum(c => c.Importe),
            ExternalReference = $"Order-{primera.SocioId}-{primera.Ano}-{string.Join("-", cuotas.Select(c => (int)c.Mes))}",
            Items = cuotas.Select(c => new Items
            {
                SkuNumber = $"{c.Id}s{c.SocioId}a{c.Ano}m{(int)c.Mes}",
                Title = $"{c.Id}s{c.SocioId}a{c.Ano}m{(int)c.Mes}",
                Description = $"Pago de la cuota {(int)c.Mes} del año {c.Ano}",
                TotalAmount = c.Importe,
                UnitPrice = c.Importe,
                UnitMeasure = "unit",
                Quantity = 1
            }).ToList()
        };

        await _mercadoPagoService.CrearOrdenAsync(externalPosId, crearOrdenRequest);
    }
}

public record ManageOrdenNotificationCommand(int Id) : IRequest;

public class ManageOrdenNotificationCommandHandler : IRequestHandler<ManageOrdenNotificationCommand>
{
    private readonly IMercadoPagoService _mercadoPagoService;
    private readonly ISender _sender;
    private readonly ILogger<ManageOrdenNotificationCommandHandler> _logger;

    public ManageOrdenNotificationCommandHandler(IMercadoPagoService mercadoPagoService, ISender sender, ILogger<ManageOrdenNotificationCommandHandler> logger)
    {
        _mercadoPagoService = mercadoPagoService;
        _sender = sender;
        _logger = logger;
    }

    public async Task Handle(ManageOrdenNotificationCommand request, CancellationToken cancellationToken)
    {
        var merchantOrder = await _mercadoPagoService.ObtenerOrdenAsync(request.Id);

        if (merchantOrder is null)
        {
            _logger.LogInformation(">>> MercadoPago: la MerchantOrder obtenida por la notificación es nula");
            return;
        }

        var cuotasIds = merchantOrder.Items
            .Select(i => int.TryParse(i.Title?.Split('s').FirstOrDefault(), out var id) ? id : 0)
            .Where(id => id > 0)
            .ToList();

        switch (merchantOrder.Status)
        {
            case MerchantOrderStatus.Opened:
                await _sender.Send(new MarcarCuotasQrCreadoCommand { CuotasIds = cuotasIds }, cancellationToken);
                break;

            case MerchantOrderStatus.Closed:
                await _sender.Send(new CobrarCuotasQrCommand { CuotasIds = cuotasIds }, cancellationToken);
                break;

            case MerchantOrderStatus.Expired:
                await _sender.Send(new MarcarCuotasImpagasCommand { CuotasIds = cuotasIds }, cancellationToken);
                break;
        }
    }
}

[Authorize(Permissions = Permissions.VerMercadoPago)]
public record ObtenerOrdenesQuery(string ExternalPosId) : IRequest<ObtenerOrdenesResponse>;

public class ObtenerOrdenesQueryHandler : IRequestHandler<ObtenerOrdenesQuery, ObtenerOrdenesResponse>
{
    private readonly IMercadoPagoService _mercadoPagoService;

    public ObtenerOrdenesQueryHandler(IMercadoPagoService mercadoPagoService)
    {
        _mercadoPagoService = mercadoPagoService;
    }

    public async Task<ObtenerOrdenesResponse> Handle(ObtenerOrdenesQuery request, CancellationToken cancellationToken)
    {
        return await _mercadoPagoService.ObtenerOrdenesAsync(request.ExternalPosId);
    }
}
