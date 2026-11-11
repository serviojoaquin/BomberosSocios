using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;
using MercadoPagoExtension.Contracts.Subscripciones;
using MercadoPagoExtension.Entities;

namespace BaseApp.Application.Suscripciones;

public class SuscripcionDto
{
    public string? Id { get; init; }

    public string? Reason { get; init; }

    public string? External_reference { get; init; }

    public string? Init_point { get; init; }

    public int Payer_id { get; init; }

    public string? Payer_first_name { get; init; }

    public string? Payer_last_name { get; init; }

    public string? Payment_method_id { get; init; }

    public DateTime Next_payment_date { get; init; }

    public DateTime Date_created { get; init; }

    public string? Status { get; init; }

    public static SuscripcionDto From(Suscripcion s)
    {
        return new SuscripcionDto
        {
            Id = s.Id,
            Reason = s.Reason,
            External_reference = s.External_reference,
            Init_point = s.Init_point,
            Payer_id = s.Payer_id,
            Payer_first_name = s.Payer_first_name,
            Payer_last_name = s.Payer_last_name,
            Payment_method_id = s.Payment_method_id,
            Next_payment_date = s.Next_payment_date,
            Date_created = s.Date_created,
            Status = s.Status
        };
    }
}

public class SuscripcionesVm
{
    public IReadOnlyCollection<SuscripcionDto> Suscripciones { get; init; } = [];
}

public class CategoriaSuscripcionDto
{
    public int Id { get; init; }

    public string Descripcion { get; init; } = string.Empty;

    public decimal Importe { get; init; }
}

public class RegisterSuscripcionVm
{
    public IReadOnlyCollection<CategoriaSuscripcionDto> Categorias { get; init; } = [];
}

public class CreateSuscripcionResponse
{
    public bool Success { get; init; }
}

[Authorize(Permissions = Permissions.VerMercadoPago)]
public record GetSuscripcionesQuery : IRequest<SuscripcionesVm>;

public class GetSuscripcionesQueryHandler : IRequestHandler<GetSuscripcionesQuery, SuscripcionesVm>
{
    private readonly IMercadoPagoService _mercadoPagoService;

    public GetSuscripcionesQueryHandler(IMercadoPagoService mercadoPagoService)
    {
        _mercadoPagoService = mercadoPagoService;
    }

    public async Task<SuscripcionesVm> Handle(GetSuscripcionesQuery request, CancellationToken cancellationToken)
    {
        var respuesta = await _mercadoPagoService.BuscarEnSubscripcionesAsync(100);

        return new SuscripcionesVm
        {
            Suscripciones = respuesta?.Results?.Select(SuscripcionDto.From).ToList() ?? []
        };
    }
}

[Authorize(Permissions = Permissions.VerMercadoPago)]
public record RegisterSuscripcionQuery : IRequest<RegisterSuscripcionVm>;

public class RegisterSuscripcionQueryHandler : IRequestHandler<RegisterSuscripcionQuery, RegisterSuscripcionVm>
{
    private readonly IApplicationDbContext _context;

    public RegisterSuscripcionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RegisterSuscripcionVm> Handle(RegisterSuscripcionQuery request, CancellationToken cancellationToken)
    {
        var categorias = await _context.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Importe1)
            .ToListAsync(cancellationToken);

        return new RegisterSuscripcionVm
        {
            Categorias = categorias
                .Select(c => new CategoriaSuscripcionDto { Id = c.Id, Descripcion = c.Descripcion, Importe = c.Importe1 })
                .ToList()
        };
    }
}

[Authorize(Permissions = Permissions.VerMercadoPago)]
public record CreateSuscripcionCommand : IRequest<CreateSuscripcionResponse>
{
    public decimal Importe { get; init; }

    public int CategoriaId { get; init; }

    public string BaseUrl { get; init; } = string.Empty;
}

public class CreateSuscripcionCommandHandler : IRequestHandler<CreateSuscripcionCommand, CreateSuscripcionResponse>
{
    private readonly IMercadoPagoService _mercadoPagoService;
    private readonly TimeProvider _timeProvider;

    public CreateSuscripcionCommandHandler(IMercadoPagoService mercadoPagoService, TimeProvider timeProvider)
    {
        _mercadoPagoService = mercadoPagoService;
        _timeProvider = timeProvider;
    }

    public async Task<CreateSuscripcionResponse> Handle(CreateSuscripcionCommand request, CancellationToken cancellationToken)
    {
        await _mercadoPagoService.CrearSuscripcionAsync(new CrearSuscripcionRequest
        {
            Back_url = $"{request.BaseUrl}/suscripciones/gracias",
            Payer_email = "javiergardella@gmail.com",
            External_reference = string.Empty,
            Reason = $"CategoriaId_{request.CategoriaId}",
            Status = "pending",
            Auto_recurring = new AutoRecurring
            {
                Currency_id = "ARS",
                Transaction_amount = request.Importe,
                Frequency = 1,
                Frequency_type = "months",
                Start_date = _timeProvider.GetLocalNow().DateTime
            }
        });

        return new CreateSuscripcionResponse { Success = true };
    }
}
