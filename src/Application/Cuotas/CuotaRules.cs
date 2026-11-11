using System.ComponentModel;
using System.Reflection;
using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Models;
using BaseApp.Domain.Entities;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Cuotas;

public static class CuotaRules
{
    public static string Descripcion(CuotaEstado estado)
    {
        return typeof(CuotaEstado).GetField(estado.ToString())?
            .GetCustomAttribute<DescriptionAttribute>()?.Description ?? estado.ToString();
    }

    public static async Task<List<Cuota>> CargarCuotasPropiasAsync(IApplicationDbContext context, ICobradorActual cobradorActual, IReadOnlyCollection<int> cuotasIds, CancellationToken cancellationToken)
    {
        var cuotas = await context.Cuotas
            .Include(c => c.Socio)
            .Where(c => cuotasIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        if (cuotas.Count != cuotasIds.Distinct().Count())
        {
            Result.Failure(["Alguna de las cuotas indicadas no existe."]).ThrowIfFailed();
        }

        var propio = await cobradorActual.GetCobradorIdAsync(cancellationToken);

        if (propio.HasValue && cuotas.Any(c => c.Socio.CobradorId != propio.Value))
        {
            Result.Failure(["Alguna de las cuotas pertenece a un socio de otro cobrador."]).ThrowIfFailed();
        }

        return cuotas;
    }
}
