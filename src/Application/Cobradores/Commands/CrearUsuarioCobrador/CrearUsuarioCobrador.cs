using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Constants;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Cobradores.Commands.CrearUsuarioCobrador;

[Authorize(Permissions = Permissions.ManageSecurity)]
public record CrearUsuarioCobradorCommand : IRequest<string>
{
    public int Id { get; init; }

    public string? Password { get; init; }
}

public class CrearUsuarioCobradorCommandValidator : AbstractValidator<CrearUsuarioCobradorCommand>
{
    public CrearUsuarioCobradorCommandValidator()
    {
        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("La contraseña inicial es requerida.")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");
    }
}

public class CrearUsuarioCobradorCommandHandler : IRequestHandler<CrearUsuarioCobradorCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public CrearUsuarioCobradorCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<string> Handle(CrearUsuarioCobradorCommand request, CancellationToken cancellationToken)
    {
        var cobrador = await _context.Cobradores
            .FindAsync([request.Id], cancellationToken);

        Guard.Against.NotFound(request.Id, cobrador);

        if (cobrador.UsuarioId is not null)
        {
            Result.Failure(["El cobrador ya tiene un usuario asignado."]).ThrowIfFailed();
        }

        if (string.IsNullOrWhiteSpace(cobrador.Email))
        {
            Result.Failure(["El cobrador no tiene email cargado; es necesario para crear el usuario."]).ThrowIfFailed();
        }

        var yaVinculado = await _context.Cobradores
            .AnyAsync(c => c.Id != cobrador.Id && c.Email == cobrador.Email && c.UsuarioId != null, cancellationToken);

        if (yaVinculado)
        {
            Result.Failure(["Ya existe otro cobrador con ese email y usuario asignado."]).ThrowIfFailed();
        }

        var usuarioId = await _identityService.GetUserIdByEmailAsync(cobrador.Email!);

        if (usuarioId is null)
        {
            var (result, nuevoId) = await _identityService.CreateUserAsync(cobrador.Email!, request.Password!);

            result.ThrowIfFailed();

            usuarioId = nuevoId;
        }

        var usuarios = await _identityService.GetUsersAsync();
        var rolesActuales = usuarios.FirstOrDefault(u => u.Id == usuarioId)?.Roles ?? [];
        var rolesResult = await _identityService.UpdateUserRolesAsync(usuarioId, rolesActuales.Append(Domain.Constants.Roles.Cobrador).Distinct().ToList());

        rolesResult.ThrowIfFailed();

        cobrador.UsuarioId = usuarioId;

        await _context.SaveChangesAsync(cancellationToken);

        return usuarioId;
    }
}
