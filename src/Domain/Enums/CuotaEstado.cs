using System.ComponentModel;

namespace BaseApp.Domain.Enums;

public enum CuotaEstado
{
    [Description("Impaga")]
    Impaga = 1,

    [Description("Cobrada")]
    Cobrada = 2,

    [Description("Bonificada")]
    Bonificada = 3,

    [Description("Inválida")]
    Invalida = 4,

    [Description("QR generado")]
    QrCreado = 5,

    [Description("Cobrada por QR")]
    CobradaQr = 6
}
