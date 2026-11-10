using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Entities;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Categorias.Commands.CreateCategoria;

[Authorize(Permissions = Permissions.CrearCategorias)]
public record CreateCategoriaCommand : IRequest<int>
{
    public string? Descripcion { get; init; }

    public IReadOnlyList<decimal> Importes { get; init; } = [];
}

public class CreateCategoriaCommandValidator : AbstractValidator<CreateCategoriaCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoriaCommandValidator(IApplicationDbContext context)
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

    public async Task<bool> SerUnica(string? descripcion, CancellationToken cancellationToken)
    {
        return !await _context.Categorias
            .AnyAsync(c => c.Descripcion == descripcion, cancellationToken);
    }
}

public class CreateCategoriaCommandHandler : IRequestHandler<CreateCategoriaCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoriaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateCategoriaCommand request, CancellationToken cancellationToken)
    {
        var entity = new Categoria
        {
            Descripcion = request.Descripcion!
        };

        entity.SetImportes(request.Importes);

        _context.Categorias.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
