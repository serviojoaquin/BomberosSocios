using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Entities;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Socios.Queries.GetSocios;

public class SocioDto
{
    public int Id { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public int NumeroDocumento { get; init; }

    public DateTime FechaAfiliacion { get; init; }

    public int CategoriaId { get; init; }

    public string Categoria { get; init; } = string.Empty;

    public int CobradorId { get; init; }

    public string Cobrador { get; init; } = string.Empty;

    public string Domicilio { get; init; } = string.Empty;

    public string? DomicilioDeCobro { get; init; }

    public string? Telefono { get; init; }

    public string? Celular { get; init; }

    public string? Email { get; init; }

    public string? Web { get; init; }

    public bool Pasivo { get; init; }

    public string Estado { get; init; } = string.Empty;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Socio, SocioDto>()
                .ForMember(d => d.Categoria, opt => opt.MapFrom(s => s.Categoria.Descripcion))
                .ForMember(d => d.Cobrador, opt => opt.MapFrom(s => s.Cobrador.Apellido + ", " + s.Cobrador.Nombre))
                .ForMember(d => d.Pasivo, opt => opt.MapFrom(s => s.Estado == SocioEstado.Pasivo))
                .ForMember(d => d.Estado, opt => opt.MapFrom(s => s.Estado == SocioEstado.Pasivo ? "Pasivo" : "Activo"));
        }
    }
}

public class SociosVm
{
    public IReadOnlyCollection<SocioDto> Socios { get; init; } = [];

    public IReadOnlyCollection<LookupDto> Categorias { get; init; } = [];

    public IReadOnlyCollection<LookupDto> Cobradores { get; init; } = [];

    public int? CobradorActualId { get; init; }
}

[Authorize(Permissions = Permissions.VerSocios)]
public record GetSociosQuery : IRequest<SociosVm>;

public class GetSociosQueryHandler : IRequestHandler<GetSociosQuery, SociosVm>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICobradorActual _cobradorActual;

    public GetSociosQueryHandler(IApplicationDbContext context, IMapper mapper, ICobradorActual cobradorActual)
    {
        _context = context;
        _mapper = mapper;
        _cobradorActual = cobradorActual;
    }

    public async Task<SociosVm> Handle(GetSociosQuery request, CancellationToken cancellationToken)
    {
        var propio = await _cobradorActual.GetCobradorIdAsync(cancellationToken);

        var sociosQuery = _context.Socios.AsNoTracking();

        if (propio.HasValue)
        {
            sociosQuery = sociosQuery.Where(s => s.CobradorId == propio.Value);
        }

        return new SociosVm
        {
            Socios = await sociosQuery
                .OrderBy(s => s.Nombre)
                .ProjectTo<SocioDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken),
            Categorias = await _context.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Descripcion)
                .Select(c => new LookupDto { Id = c.Id, Title = c.Descripcion })
                .ToListAsync(cancellationToken),
            Cobradores = await _context.Cobradores
                .AsNoTracking()
                .Where(c => !propio.HasValue || c.Id == propio.Value)
                .OrderBy(c => c.Apellido).ThenBy(c => c.Nombre)
                .Select(c => new LookupDto { Id = c.Id, Title = c.Apellido + ", " + c.Nombre })
                .ToListAsync(cancellationToken),
            CobradorActualId = propio
        };
    }
}
