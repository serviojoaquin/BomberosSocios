using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Socios.Commands.UpdateSocio;

[Authorize(Permissions = Permissions.EditarSocios)]
public record UpdateSocioCommand : IRequest, ISocioDatos
{
    public int Id { get; init; }

    public string? Nombre { get; init; }

    public int NumeroDocumento { get; init; }

    public DateTime? FechaAfiliacion { get; init; }

    public int CategoriaId { get; init; }

    public int CobradorId { get; init; }

    public string? Domicilio { get; init; }

    public string? DomicilioDeCobro { get; init; }

    public string? Telefono { get; init; }

    public string? Celular { get; init; }

    public string? Email { get; init; }

    public string? Web { get; init; }

    public bool Pasivo { get; init; }
}

public class UpdateSocioCommandValidator : AbstractValidator<UpdateSocioCommand>
{
    public UpdateSocioCommandValidator()
    {
        SocioRules.Apply(this);
    }
}

public class UpdateSocioCommandHandler : IRequestHandler<UpdateSocioCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICobradorActual _cobradorActual;

    public UpdateSocioCommandHandler(IApplicationDbContext context, ICobradorActual cobradorActual)
    {
        _context = context;
        _cobradorActual = cobradorActual;
    }

    public async Task Handle(UpdateSocioCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Socios
            .FindAsync([request.Id], cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        var propio = await _cobradorActual.GetCobradorIdAsync(cancellationToken);

        if (propio.HasValue && entity.CobradorId != propio.Value)
        {
            Result.Failure(["El socio pertenece a otro cobrador."]).ThrowIfFailed();
        }

        await SocioRules.ValidarReferenciasAsync(_context, _cobradorActual, request.CategoriaId, request.CobradorId, cancellationToken);

        entity.Nombre = request.Nombre!.Trim();
        entity.NumeroDocumento = request.NumeroDocumento;
        entity.FechaAfiliacion = request.FechaAfiliacion ?? entity.FechaAfiliacion;
        entity.CategoriaId = request.CategoriaId;
        entity.CobradorId = request.CobradorId;
        entity.Domicilio = request.Domicilio!.Trim();
        entity.DomicilioDeCobro = SocioRules.Limpiar(request.DomicilioDeCobro);
        entity.Telefono = SocioRules.Limpiar(request.Telefono);
        entity.Celular = SocioRules.Limpiar(request.Celular);
        entity.Email = SocioRules.Limpiar(request.Email);
        entity.Web = SocioRules.Limpiar(request.Web);
        entity.Estado = request.Pasivo ? SocioEstado.Pasivo : SocioEstado.Activo;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
