namespace BaseApp.Application.Common.Interfaces;

public interface ICobradorActual
{
    Task<int?> GetCobradorIdAsync(CancellationToken cancellationToken);
}
