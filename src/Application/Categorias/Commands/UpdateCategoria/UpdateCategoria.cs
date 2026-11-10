using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Entities;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Categorias.Commands.UpdateCategoria;

[Authorize(Permissions = Permissions.EditarCategorias)]
public record UpdateCategoriaCommand : IRequest
{
    public int Id { get; init; }

    public string? Descripcion { get; init; }

    public IReadOnlyList<decimal> Importes { get; init; } = [];
}

public class UpdateCategoriaCommandValidator : AbstractValidator<UpdateCategoriaCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoriaCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Descripcion)
            .NotEmpty().WithMessage("La descripción es requerida.")
            .MaximumLength(200).WithMessage("La descripción no puede exceder los 200 caracteres.")
            .MustAsync(SerUnica).WithMessage("Ya existe una categoría con esa descripción.").WithErrorCode("Unique");

        RuleFor(v => v.Importes)
            .Must(i => i.Count == Categoria.MesesPorAno).WithMessage("Deben informarse los 12 importes mensuales.");

        RuleForEach(v => v.Importes)
            .GreaterThanOrEqualTo(0).WithMessage("Los importes no pueden ser negativos.")
            .PrecisionScale(8, 2, false).WithMessage("Los importes admiten hasta 6 cifras y 2 decimales.");
    }

    public async Task<bool> SerUnica(UpdateCategoriaCommand model, string? descripcion, CancellationToken cancellationToken)
    {
        return !await _context.Categorias
            .Where(c => c.Id != model.Id)
            .AnyAsync(c => c.Descripcion == descripcion, cancellationToken);
    }
}

public class UpdateCategoriaCommandHandler : IRequestHandler<UpdateCategoriaCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoriaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCategoriaCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Categorias
            .FindAsync([request.Id], cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Descripcion = request.Descripcion!;
        entity.SetImportes(request.Importes);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
