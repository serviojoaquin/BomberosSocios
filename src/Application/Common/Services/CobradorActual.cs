using BaseApp.Application.Common.Interfaces;

namespace BaseApp.Application.Common.Services;

public class CobradorActual : ICobradorActual
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private int? _cobradorId;
    private bool _resuelto;

    public CobradorActual(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<int?> GetCobradorIdAsync(CancellationToken cancellationToken)
    {
        if (_resuelto)
        {
            return _cobradorId;
        }

        _resuelto = true;

        var esSoloCobrador = _user.Roles is not null
            && _user.Roles.Contains(Domain.Constants.Roles.Cobrador)
            && !_user.Roles.Contains(Domain.Constants.Roles.Administrator);

        if (!esSoloCobrador || _user.Id is null)
        {
            return null;
        }

        _cobradorId = await _context.Cobradores
            .Where(c => c.UsuarioId == _user.Id)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return _cobradorId;
    }
}
