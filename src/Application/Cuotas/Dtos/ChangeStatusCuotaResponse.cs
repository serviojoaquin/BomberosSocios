namespace BaseApp.Application.Cuotas.Dtos;

public class ChangeStatusCuotaResponse
{
    public int CuotaEstadoId { get; init; }

    public DateTime? FechaDeCobro { get; init; }

    public DateTime? FechaBonificada { get; init; }

    public int CuotasModifiedCount { get; init; }
}
