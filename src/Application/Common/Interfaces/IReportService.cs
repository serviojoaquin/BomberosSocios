using BaseApp.Domain.Entities;

namespace BaseApp.Application.Common.Interfaces;

public interface IReportService
{
    byte[] GeneratePdfReportCuotasSocio(Socio socio, IReadOnlyCollection<Cuota> cuotas, int year);
}
