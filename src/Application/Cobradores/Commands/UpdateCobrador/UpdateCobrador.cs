using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Cobradores.Commands.UpdateCobrador;

[Authorize(Permissions = Permissions.EditarCobradores)]
public record UpdateCobradorCommand : IRequest
{
    public int Id { get; init; }

    public string? Nombre { get; init; }

    public string? Apellido { get; init; }

    public string? Telefono { get; init; }

    public string? Celular { get; init; }

    public string? Email { get; init; }

    public decimal? Comision { get; init; }
}

public class UpdateCobradorCommandValidator : AbstractValidator<UpdateCobradorCommand>
{
    public UpdateCobradorCommandValidator()
    {
        RuleFor(v => v.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder los 200 caracteres.");

        RuleFor(v => v.Apellido)
            .NotEmpty().WithMessage("El apellido es requerido.")
            .MaximumLength(200).WithMessage("El apellido no puede exceder los 200 caracteres.");

        RuleFor(v => v.Email)
            .MaximumLength(200).WithMessage("El email no puede exceder los 200 caracteres.")
            .EmailAddress().When(v => !string.IsNullOrWhiteSpace(v.Email)).WithMessage("El email no es válido.");

        RuleFor(v => v.Telefono)
            .MaximumLength(50).WithMessage("El teléfono no puede exceder los 50 caracteres.");

        RuleFor(v => v.Celular)
            .MaximumLength(50).WithMessage("El celular no puede exceder los 50 caracteres.");

        RuleFor(v => v.Comision)
            .InclusiveBetween(0, 100).When(v => v.Comision.HasValue).WithMessage("La comisión debe estar entre 0 y 100.");
    }
}

public class UpdateCobradorCommandHandler : IRequestHandler<UpdateCobradorCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCobradorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCobradorCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Cobradores
            .FindAsync([request.Id], cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Nombre = request.Nombre!.Trim();
        entity.Apellido = request.Apellido!.Trim();
        entity.Telefono = request.Telefono;
        entity.Celular = request.Celular;
        entity.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        entity.Comision = request.Comision;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
