using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Entities;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Cobradores.Queries.GetCobradores;

public class CobradorDto
{
    private const int SociosPorParte = 200;

    public int Id { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public string Apellido { get; init; } = string.Empty;

    public string? Telefono { get; init; }

    public string? Celular { get; init; }

    public string? Email { get; init; }

    public decimal? Comision { get; init; }

    public bool TieneUsuario { get; init; }

    public int SociosCount { get; init; }

    public int CuotasCobradasCount { get; init; }

    public int CantidadBotones => (int)Math.Ceiling(SociosCount / (decimal)SociosPorParte);

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Cobrador, CobradorDto>()
                .ForMember(d => d.TieneUsuario, opt => opt.MapFrom(s => s.UsuarioId != null))
                .ForMember(d => d.SociosCount, opt => opt.MapFrom(s => s.Socios.Count))
                .ForMember(d => d.CuotasCobradasCount, opt => opt.MapFrom(s => s.CuotasCobradas.Count));
        }
    }
}

public class CobradoresVm
{
    public IReadOnlyCollection<CobradorDto> Cobradores { get; init; } = [];

    public int InitialYear { get; init; }
}

[Authorize(Permissions = Permissions.VerCobradores)]
public record GetCobradoresQuery : IRequest<CobradoresVm>;

public class GetCobradoresQueryHandler : IRequestHandler<GetCobradoresQuery, CobradoresVm>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICobradorActual _cobradorActual;
    private readonly TimeProvider _timeProvider;

    public GetCobradoresQueryHandler(IApplicationDbContext context, IMapper mapper, ICobradorActual cobradorActual, TimeProvider timeProvider)
    {
        _context = context;
        _mapper = mapper;
        _cobradorActual = cobradorActual;
        _timeProvider = timeProvider;
    }

    public async Task<CobradoresVm> Handle(GetCobradoresQuery request, CancellationToken cancellationToken)
    {
        var propio = await _cobradorActual.GetCobradorIdAsync(cancellationToken);

        return new CobradoresVm
        {
            Cobradores = await _context.Cobradores
                .AsNoTracking()
                .Where(c => !propio.HasValue || c.Id == propio.Value)
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .ProjectTo<CobradorDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken),
            InitialYear = _timeProvider.GetLocalNow().Year
        };
    }
}
