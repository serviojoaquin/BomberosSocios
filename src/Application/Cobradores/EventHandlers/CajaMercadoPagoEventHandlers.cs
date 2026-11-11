using BaseApp.Application.Common.Interfaces;
using BaseApp.Domain.Events;
using MercadoPagoExtension.Contracts.Cajas;
using Microsoft.Extensions.Logging;

namespace BaseApp.Application.Cobradores.EventHandlers;

public class CrearCajaEventHandler : INotificationHandler<CrearCajaEvent>
{
    private readonly ILogger<CrearCajaEventHandler> _logger;
    private readonly IMercadoPagoService _mercadoPagoService;

    public CrearCajaEventHandler(ILogger<CrearCajaEventHandler> logger, IMercadoPagoService mercadoPagoService)
    {
        _logger = logger;
        _mercadoPagoService = mercadoPagoService;
    }

    public async Task Handle(CrearCajaEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("BaseApp Domain Event: {DomainEvent}", notification.GetType().Name);

        await _mercadoPagoService.CrearCajaAsync(new CrearCajaRequest
        {
            Name = notification.Name,
            External_id = notification.ExternalId
        });
    }
}

public class EliminarCajaEventHandler : INotificationHandler<EliminarCajaEvent>
{
    private readonly ILogger<EliminarCajaEventHandler> _logger;
    private readonly IMercadoPagoService _mercadoPagoService;

    public EliminarCajaEventHandler(ILogger<EliminarCajaEventHandler> logger, IMercadoPagoService mercadoPagoService)
    {
        _logger = logger;
        _mercadoPagoService = mercadoPagoService;
    }

    public async Task Handle(EliminarCajaEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("BaseApp Domain Event: {DomainEvent}", notification.GetType().Name);

        var cajas = await _mercadoPagoService.BuscarEnCajasAsync($"CajaCobrador{notification.CobradorId}");

        if (cajas.Results is { Count: > 0 })
        {
            await _mercadoPagoService.DeleteCajaAsync(cajas.Results.First().Id);
        }
    }
}
