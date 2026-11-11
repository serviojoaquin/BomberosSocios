namespace BaseApp.Domain.Entities;

public class Reporte : BaseAuditableEntity
{
    public byte[] ReporteRaw { get; set; } = [];

    public int EntidadId { get; set; }

    public ReporteTipo ReporteTipo { get; set; }

    public short? Ano { get; set; }
}
