using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Socios.Commands.UpdateDatosAfiliacion;

[Authorize(Permissions = Permissions.EditarSocios)]
public record UpdateDatosAfiliacionCommand : IRequest
{
    public IReadOnlyCollection<int> SociosIds { get; init; } = [];

    public int? CategoriaId { get; init; }

    public int? CobradorId { get; init; }

    public bool? Pasivo { get; init; }
}

public class UpdateDatosAfiliacionCommandValidator : AbstractValidator<UpdateDatosAfiliacionCommand>
{
    public UpdateDatosAfiliacionCommandValidator()
    {
        RuleFor(v => v.SociosIds)
            .NotEmpty().WithMessage("Seleccioná al menos un socio.");

        RuleFor(v => v)
            .Must(v => v.CategoriaId.HasValue || v.CobradorId.HasValue || v.Pasivo.HasValue)
            .WithMessage("Indicá al menos un dato a modificar.")
            .WithName("Datos");
    }
}

public class UpdateDatosAfiliacionCommandHandler : IRequestHandler<UpdateDatosAfiliacionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICobradorActual _cobradorActual;

    public UpdateDatosAfiliacionCommandHandler(IApplicationDbContext context, ICobradorActual cobradorActual)
    {
        _context = context;
        _cobradorActual = cobradorActual;
    }

    public async Task Handle(UpdateDatosAfiliacionCommand request, CancellationToken cancellationToken)
    {
        var socios = await _context.Socios
            .Where(s => request.SociosIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        var propio = await _cobradorActual.GetCobradorIdAsync(cancellationToken);

        if (propio.HasValue && socios.Any(s => s.CobradorId != propio.Value))
        {
            Result.Failure(["Alguno de los socios pertenece a otro cobrador."]).ThrowIfFailed();
        }

        if (request.CategoriaId.HasValue && !await _context.Categorias.AnyAsync(c => c.Id == request.CategoriaId.Value, cancellationToken))
        {
            Result.Failure(["La categoría indicada no existe."]).ThrowIfFailed();
        }

        if (request.CobradorId.HasValue)
        {
            if (!await _context.Cobradores.AnyAsync(c => c.Id == request.CobradorId.Value, cancellationToken))
            {
                Result.Failure(["El cobrador indicado no existe."]).ThrowIfFailed();
            }

            if (propio.HasValue && propio.Value != request.CobradorId.Value)
            {
                Result.Failure(["Un cobrador solo puede asignarse socios a sí mismo."]).ThrowIfFailed();
            }
        }

        foreach (var socio in socios)
        {
            socio.CategoriaId = request.CategoriaId ?? socio.CategoriaId;
            socio.CobradorId = request.CobradorId ?? socio.CobradorId;
            if (request.Pasivo.HasValue)
            {
                socio.Estado = request.Pasivo.Value ? SocioEstado.Pasivo : SocioEstado.Activo;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
