using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;
using MercadoPagoExtension.Contracts.Cajas;
using MercadoPagoExtension.Entities;

namespace BaseApp.Application.Cajas;

public class QrDto
{
    public string? Image { get; init; }

    public string? Template_document { get; init; }

    public string? Template_image { get; init; }
}

public class CajaDto
{
    public int Id { get; init; }

    public QrDto? Qr { get; init; }

    public DateTime Date_created { get; init; }

    public DateTime Date_last_updated { get; init; }

    public string? Uuid { get; init; }

    public int User_id { get; init; }

    public string? Name { get; init; }

    public bool Fixed_amount { get; init; }

    public string? External_id { get; init; }

    public string? Site { get; init; }

    public string? Qr_code { get; init; }

    public string? Status { get; init; }

    public string? Url { get; init; }

    public string? Store_id { get; init; }

    public string? External_store_id { get; init; }

    public int Category { get; init; }

    public static CajaDto From(Caja caja, string? status = null)
    {
        return new CajaDto
        {
            Id = caja.Id,
            Qr = caja.Qr is null ? null : new QrDto { Image = caja.Qr.Image, Template_document = caja.Qr.Template_document, Template_image = caja.Qr.Template_image },
            Date_created = caja.Date_created,
            Date_last_updated = caja.Date_last_updated,
            Uuid = caja.Uuid,
            User_id = caja.User_id,
            Name = caja.Name,
            Fixed_amount = caja.Fixed_amount,
            External_id = caja.External_id,
            Site = caja.Site,
            Qr_code = caja.Qr_code,
            Status = status ?? caja.Status,
            Url = caja.Url,
            Store_id = caja.Store_id,
            External_store_id = caja.External_store_id,
            Category = caja.Category
        };
    }
}

public class CajasVm
{
    public IReadOnlyCollection<CajaDto> Cajas { get; init; } = [];
}

public class CajaVm
{
    public CajaDto? Caja { get; init; }
}

[Authorize(Permissions = Permissions.VerMercadoPago)]
public record GetCajasQuery : IRequest<CajasVm>;

public class GetCajasQueryHandler : IRequestHandler<GetCajasQuery, CajasVm>
{
    private readonly IMercadoPagoService _mercadoPagoService;

    public GetCajasQueryHandler(IMercadoPagoService mercadoPagoService)
    {
        _mercadoPagoService = mercadoPagoService;
    }

    public async Task<CajasVm> Handle(GetCajasQuery request, CancellationToken cancellationToken)
    {
        var respuesta = await _mercadoPagoService.BuscarEnCajasAsync(null, null, null);
        var cajas = new List<CajaDto>();

        foreach (var caja in respuesta.Results ?? [])
        {
            var detalle = await _mercadoPagoService.ObtenerCajaAsync(caja.Id.ToString());
            cajas.Add(CajaDto.From(caja, detalle.Status));
        }

        return new CajasVm { Cajas = cajas };
    }
}

[Authorize(Permissions = Permissions.PagosMercadoPago)]
public record GetCurrentCajaQuery : IRequest<CajaVm>;

public class GetCurrentCajaQueryHandler : IRequestHandler<GetCurrentCajaQuery, CajaVm>
{
    private readonly IMercadoPagoService _mercadoPagoService;
    private readonly ICobradorActual _cobradorActual;

    public GetCurrentCajaQueryHandler(IMercadoPagoService mercadoPagoService, ICobradorActual cobradorActual)
    {
        _mercadoPagoService = mercadoPagoService;
        _cobradorActual = cobradorActual;
    }

    public async Task<CajaVm> Handle(GetCurrentCajaQuery request, CancellationToken cancellationToken)
    {
        var propio = await _cobradorActual.GetCobradorIdAsync(cancellationToken);
        var cajas = await _mercadoPagoService.BuscarEnCajasAsync(propio.HasValue ? $"CajaCobrador{propio.Value}" : "CajaPrincipal");
        var caja = cajas.Results?.FirstOrDefault();

        return new CajaVm { Caja = caja is null ? null : CajaDto.From(caja) };
    }
}

[Authorize(Permissions = Permissions.AdministrarMercadoPago)]
public record CrearCajaPrincipalCommand : IRequest<CajaDto?>;

public class CrearCajaPrincipalCommandHandler : IRequestHandler<CrearCajaPrincipalCommand, CajaDto?>
{
    private readonly IMercadoPagoService _mercadoPagoService;

    public CrearCajaPrincipalCommandHandler(IMercadoPagoService mercadoPagoService)
    {
        _mercadoPagoService = mercadoPagoService;
    }

    public async Task<CajaDto?> Handle(CrearCajaPrincipalCommand request, CancellationToken cancellationToken)
    {
        var existentes = await _mercadoPagoService.BuscarEnCajasAsync("CajaPrincipal");

        if (existentes.Results is { Count: > 0 })
        {
            return null;
        }

        var caja = await _mercadoPagoService.CrearCajaAsync(new CrearCajaRequest
        {
            Name = "Caja Principal",
            External_id = "CajaPrincipal"
        });

        return caja is null || caja.Id == 0 ? null : CajaDto.From(caja);
    }
}

[Authorize(Permissions = Permissions.AdministrarMercadoPago)]
public record DeleteCajaCommand(int Id) : IRequest;

public class DeleteCajaCommandHandler : IRequestHandler<DeleteCajaCommand>
{
    private readonly IMercadoPagoService _mercadoPagoService;

    public DeleteCajaCommandHandler(IMercadoPagoService mercadoPagoService)
    {
        _mercadoPagoService = mercadoPagoService;
    }

    public async Task Handle(DeleteCajaCommand request, CancellationToken cancellationToken)
    {
        await _mercadoPagoService.DeleteCajaAsync(request.Id);
    }
}
