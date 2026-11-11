using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Entities;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Socios.Commands.CreateSocio;

[Authorize(Permissions = Permissions.CrearSocios)]
public record CreateSocioCommand : IRequest<int>, ISocioDatos
{
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

public class CreateSocioCommandValidator : AbstractValidator<CreateSocioCommand>
{
    public CreateSocioCommandValidator()
    {
        SocioRules.Apply(this);
    }
}

public class CreateSocioCommandHandler : IRequestHandler<CreateSocioCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICobradorActual _cobradorActual;

    public CreateSocioCommandHandler(IApplicationDbContext context, ICobradorActual cobradorActual)
    {
        _context = context;
        _cobradorActual = cobradorActual;
    }

    public async Task<int> Handle(CreateSocioCommand request, CancellationToken cancellationToken)
    {
        await SocioRules.ValidarReferenciasAsync(_context, _cobradorActual, request.CategoriaId, request.CobradorId, cancellationToken);

        var entity = new Socio
        {
            Nombre = request.Nombre!.Trim(),
            NumeroDocumento = request.NumeroDocumento,
            FechaAfiliacion = request.FechaAfiliacion ?? DateTime.Today,
            CategoriaId = request.CategoriaId,
            CobradorId = request.CobradorId,
            Domicilio = request.Domicilio!.Trim(),
            DomicilioDeCobro = SocioRules.Limpiar(request.DomicilioDeCobro),
            Telefono = SocioRules.Limpiar(request.Telefono),
            Celular = SocioRules.Limpiar(request.Celular),
            Email = SocioRules.Limpiar(request.Email),
            Web = SocioRules.Limpiar(request.Web),
            Estado = request.Pasivo ? SocioEstado.Pasivo : SocioEstado.Activo
        };

        _context.Socios.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
