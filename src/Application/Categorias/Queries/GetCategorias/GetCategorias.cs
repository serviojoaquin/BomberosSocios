using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Entities;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Categorias.Queries.GetCategorias;

public class CategoriaDto
{
    public int Id { get; init; }

    public string Descripcion { get; init; } = string.Empty;

    public IReadOnlyList<decimal> Importes { get; init; } = [];

    public int SociosCount { get; init; }

    public static CategoriaDto From(Categoria categoria, int sociosCount)
    {
        return new CategoriaDto
        {
            Id = categoria.Id,
            Descripcion = categoria.Descripcion,
            Importes = categoria.GetImportes(),
            SociosCount = sociosCount
        };
    }
}

public class CategoriasVm
{
    public IReadOnlyCollection<CategoriaDto> Categorias { get; init; } = [];
}

[Authorize(Permissions = Permissions.VerCategorias)]
public record GetCategoriasQuery : IRequest<CategoriasVm>;

public class GetCategoriasQueryHandler : IRequestHandler<GetCategoriasQuery, CategoriasVm>
{
    private readonly IApplicationDbContext _context;

    public GetCategoriasQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoriasVm> Handle(GetCategoriasQuery request, CancellationToken cancellationToken)
    {
        var categorias = await _context.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Descripcion)
            .Select(c => new { Categoria = c, SociosCount = c.Socios.Count })
            .ToListAsync(cancellationToken);

        return new CategoriasVm
        {
            Categorias = categorias.Select(c => CategoriaDto.From(c.Categoria, c.SociosCount)).ToList()
        };
    }
}
