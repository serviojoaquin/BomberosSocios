namespace BaseApp.Application.Cobradores.Queries;

public class CobradorResumenDto
{
    public int Id { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public string Apellido { get; init; } = string.Empty;

    public decimal Comision { get; init; }
}

public class SocioCuotasPivotDto
{
    public int SocioId { get; init; }

    public string SocioNombre { get; init; } = string.Empty;

    public IReadOnlyList<decimal?> Importes { get; init; } = new decimal?[12];

    public decimal Total { get; init; }

    public static IReadOnlyCollection<SocioCuotasPivotDto> Pivot(IEnumerable<(int SocioId, string SocioNombre, int Mes, decimal Importe)> cuotas)
    {
        return cuotas
            .GroupBy(c => new { c.SocioId, c.SocioNombre })
            .OrderBy(g => g.Key.SocioId)
            .Select(g =>
            {
                var importes = new decimal?[12];

                foreach (var cuota in g)
                {
                    var indice = cuota.Mes - 1;
                    importes[indice] = (importes[indice] ?? 0) + cuota.Importe;
                }

                return new SocioCuotasPivotDto
                {
                    SocioId = g.Key.SocioId,
                    SocioNombre = g.Key.SocioNombre,
                    Importes = importes,
                    Total = importes.Sum(i => i ?? 0)
                };
            })
            .ToList();
    }
}
