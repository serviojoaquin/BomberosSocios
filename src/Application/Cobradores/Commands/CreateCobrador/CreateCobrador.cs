using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Entities;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Cobradores.Commands.CreateCobrador;

[Authorize(Permissions = Permissions.CrearCobradores)]
public record CreateCobradorCommand : IRequest<int>
{
    public string? Nombre { get; init; }

    public string? Apellido { get; init; }

    public string? Telefono { get; init; }

    public string? Celular { get; init; }

    public string? Email { get; init; }

    public decimal? Comision { get; init; }
}

public class CreateCobradorCommandValidator : AbstractValidator<CreateCobradorCommand>
{
    public CreateCobradorCommandValidator()
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

public class CreateCobradorCommandHandler : IRequestHandler<CreateCobradorCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateCobradorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateCobradorCommand request, CancellationToken cancellationToken)
    {
        var entity = new Cobrador
        {
            Nombre = request.Nombre!.Trim(),
            Apellido = request.Apellido!.Trim(),
            Telefono = request.Telefono,
            Celular = request.Celular,
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            Comision = request.Comision
        };

        _context.Cobradores.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
