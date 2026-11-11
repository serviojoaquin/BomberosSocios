namespace BaseApp.Domain.Entities;

public class Socio : BaseAuditableEntity
{
    public string Nombre { get; set; } = string.Empty;

    public int NumeroDocumento { get; set; }

    public DateTime FechaAfiliacion { get; set; }

    public int CategoriaId { get; set; }

    public Categoria Categoria { get; set; } = null!;

    public int CobradorId { get; set; }

    public Cobrador Cobrador { get; set; } = null!;

    public string Domicilio { get; set; } = string.Empty;

    public string? DomicilioDeCobro { get; set; }

    public string? Telefono { get; set; }

    public string? Celular { get; set; }

    public string? Email { get; set; }

    public string? Web { get; set; }

    public SocioEstado Estado { get; set; } = SocioEstado.Activo;
}
