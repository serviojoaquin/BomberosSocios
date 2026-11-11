using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;

namespace BaseApp.Application.Socios;

public interface ISocioDatos
{
    string? Nombre { get; }
    int NumeroDocumento { get; }
    int CategoriaId { get; }
    int CobradorId { get; }
    string? Domicilio { get; }
    string? DomicilioDeCobro { get; }
    string? Telefono { get; }
    string? Celular { get; }
    string? Email { get; }
    string? Web { get; }
}

public static class SocioRules
{
    public static void Apply<T>(AbstractValidator<T> validator) where T : ISocioDatos
    {
        validator.RuleFor(v => v.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder los 200 caracteres.");

        validator.RuleFor(v => v.NumeroDocumento)
            .GreaterThan(0).WithMessage("El número de documento es requerido.");

        validator.RuleFor(v => v.CategoriaId)
            .GreaterThan(0).WithMessage("La categoría es requerida.");

        validator.RuleFor(v => v.CobradorId)
            .GreaterThan(0).WithMessage("El cobrador es requerido.");

        validator.RuleFor(v => v.Domicilio)
            .NotEmpty().WithMessage("El domicilio es requerido.")
            .MaximumLength(300).WithMessage("El domicilio no puede exceder los 300 caracteres.");

        validator.RuleFor(v => v.DomicilioDeCobro)
            .MaximumLength(300).WithMessage("El domicilio de cobro no puede exceder los 300 caracteres.");

        validator.RuleFor(v => v.Telefono)
            .MaximumLength(50).WithMessage("El teléfono no puede exceder los 50 caracteres.");

        validator.RuleFor(v => v.Celular)
            .MaximumLength(50).WithMessage("El celular no puede exceder los 50 caracteres.");

        validator.RuleFor(v => v.Email)
            .MaximumLength(200).WithMessage("El email no puede exceder los 200 caracteres.")
            .EmailAddress().When(v => !string.IsNullOrWhiteSpace(v.Email)).WithMessage("El email no es válido.");

        validator.RuleFor(v => v.Web)
            .MaximumLength(200).WithMessage("La web no puede exceder los 200 caracteres.");
    }

    public static async Task ValidarReferenciasAsync(IApplicationDbContext context, ICobradorActual cobradorActual, int categoriaId, int cobradorId, CancellationToken cancellationToken)
    {
        var errores = new List<string>();

        if (!await context.Categorias.AnyAsync(c => c.Id == categoriaId, cancellationToken))
        {
            errores.Add("La categoría indicada no existe.");
        }

        if (!await context.Cobradores.AnyAsync(c => c.Id == cobradorId, cancellationToken))
        {
            errores.Add("El cobrador indicado no existe.");
        }

        var propio = await cobradorActual.GetCobradorIdAsync(cancellationToken);

        if (propio.HasValue && propio.Value != cobradorId)
        {
            errores.Add("Un cobrador solo puede asignarse socios a sí mismo.");
        }

        if (errores.Count > 0)
        {
            Result.Failure(errores).ThrowIfFailed();
        }
    }

    public static string? Limpiar(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
