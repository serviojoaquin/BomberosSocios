using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Entities;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Cobradores.Queries.GetCobradores;

public class CobradorDto
{
    public int Id { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public string Apellido { get; init; } = string.Empty;

    public string? Telefono { get; init; }

    public string? Celular { get; init; }

    public string? Email { get; init; }

    public decimal? Comision { get; init; }

    public bool TieneUsuario { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Cobrador, CobradorDto>()
                .ForMember(d => d.TieneUsuario, opt => opt.MapFrom(s => s.UsuarioId != null));
        }
    }
}

public class CobradoresVm
{
    public IReadOnlyCollection<CobradorDto> Cobradores { get; init; } = [];
}

[Authorize(Permissions = Permissions.VerCobradores)]
public record GetCobradoresQuery : IRequest<CobradoresVm>;

public class GetCobradoresQueryHandler : IRequestHandler<GetCobradoresQuery, CobradoresVm>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCobradoresQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CobradoresVm> Handle(GetCobradoresQuery request, CancellationToken cancellationToken)
    {
        return new CobradoresVm
        {
            Cobradores = await _context.Cobradores
                .AsNoTracking()
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .ProjectTo<CobradorDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken)
        };
    }
}
