namespace BaseApp.Domain.Entities;

public class Cuota : BaseAuditableEntity
{
    public int SocioId { get; set; }

    public Socio Socio { get; set; } = null!;

    public short Ano { get; set; }

    public Mes Mes { get; set; }

    public decimal Importe { get; set; }

    public CuotaEstado Estado { get; set; } = CuotaEstado.Impaga;

    public DateTime? FechaDeCobro { get; set; }

    public DateTime? FechaBonificada { get; set; }

    public int? CobradorId { get; set; }

    public Cobrador? Cobrador { get; set; }

    public bool EstaPaga => Estado is CuotaEstado.Cobrada or CuotaEstado.CobradaQr or CuotaEstado.Bonificada;

    public void Cobrar(DateTime fecha, int cobradorId)
    {
        Estado = CuotaEstado.Cobrada;
        FechaDeCobro = fecha;
        FechaBonificada = null;
        CobradorId = cobradorId;
    }

    public void Bonificar(DateTime fecha, int? cobradorId)
    {
        Estado = CuotaEstado.Bonificada;
        FechaBonificada = fecha;
        FechaDeCobro = null;
        CobradorId = cobradorId;
    }

    public void MarcarImpaga()
    {
        Estado = CuotaEstado.Impaga;
        FechaDeCobro = null;
        FechaBonificada = null;
        CobradorId = null;
    }

    public void MarcarQrCreado()
    {
        Estado = CuotaEstado.QrCreado;
    }

    public void CobrarPorQr(DateTime fecha, int cobradorId)
    {
        Estado = CuotaEstado.CobradaQr;
        FechaDeCobro = fecha;
        FechaBonificada = null;
        CobradorId = cobradorId;
    }
}
