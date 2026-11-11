namespace BaseApp.Domain.Entities;

public class Cobrador : BaseAuditableEntity
{
    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public string? Celular { get; set; }

    public string? Email { get; set; }

    public decimal? Comision { get; set; }

    public string? UsuarioId { get; set; }

    public IList<Socio> Socios { get; private set; } = new List<Socio>();

    public IList<Cuota> CuotasCobradas { get; private set; } = new List<Cuota>();

    public string NombreCompleto => $"{Apellido}, {Nombre}";
}
