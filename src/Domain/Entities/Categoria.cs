namespace BaseApp.Domain.Entities;

public class Categoria : BaseAuditableEntity
{
    public const int MesesPorAno = 12;

    public IList<Socio> Socios { get; private set; } = new List<Socio>();

    public string Descripcion { get; set; } = string.Empty;

    public decimal Importe1 { get; set; }

    public decimal Importe2 { get; set; }

    public decimal Importe3 { get; set; }

    public decimal Importe4 { get; set; }

    public decimal Importe5 { get; set; }

    public decimal Importe6 { get; set; }

    public decimal Importe7 { get; set; }

    public decimal Importe8 { get; set; }

    public decimal Importe9 { get; set; }

    public decimal Importe10 { get; set; }

    public decimal Importe11 { get; set; }

    public decimal Importe12 { get; set; }

    public decimal[] GetImportes()
    {
        return [Importe1, Importe2, Importe3, Importe4, Importe5, Importe6, Importe7, Importe8, Importe9, Importe10, Importe11, Importe12];
    }

    public void SetImportes(IReadOnlyList<decimal> importes)
    {
        if (importes.Count != MesesPorAno)
        {
            throw new ArgumentException($"Se esperaban {MesesPorAno} importes, llegaron {importes.Count}.", nameof(importes));
        }

        Importe1 = importes[0];
        Importe2 = importes[1];
        Importe3 = importes[2];
        Importe4 = importes[3];
        Importe5 = importes[4];
        Importe6 = importes[5];
        Importe7 = importes[6];
        Importe8 = importes[7];
        Importe9 = importes[8];
        Importe10 = importes[9];
        Importe11 = importes[10];
        Importe12 = importes[11];
    }

    public decimal ImporteDelMes(int mes)
    {
        return GetImportes()[mes - 1];
    }
}
