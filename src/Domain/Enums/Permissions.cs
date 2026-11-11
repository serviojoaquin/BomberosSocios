using System.ComponentModel;

namespace BaseApp.Domain.Enums;

[Flags]
public enum Permissions
{
    None = 0,

    [Description("View sample items")]
    ViewSampleItems = 1,

    [Description("Create sample items")]
    CreateSampleItems = 2,

    [Description("Edit sample items")]
    EditSampleItems = 4,

    [Description("Delete sample items")]
    DeleteSampleItems = 8,

    [Description("Administrar seguridad")]
    ManageSecurity = 16,

    [Description("Ver categorías")]
    VerCategorias = 32,

    [Description("Crear categorías")]
    CrearCategorias = 64,

    [Description("Editar categorías")]
    EditarCategorias = 128,

    [Description("Eliminar categorías")]
    EliminarCategorias = 256,

    [Description("Ver cobradores")]
    VerCobradores = 512,

    [Description("Crear cobradores")]
    CrearCobradores = 1024,

    [Description("Editar cobradores")]
    EditarCobradores = 2048,

    [Description("Eliminar cobradores")]
    EliminarCobradores = 4096,

    [Description("Ver socios")]
    VerSocios = 8192,

    [Description("Crear socios")]
    CrearSocios = 16384,

    [Description("Editar socios")]
    EditarSocios = 32768,

    [Description("Eliminar socios")]
    EliminarSocios = 65536,

    [Description("Ver cuotas")]
    VerCuotas = 131072,

    [Description("Pagar cuotas")]
    PagarCuotas = 262144,

    [Description("Administrar cuotas")]
    AdministrarCuotas = 524288,

    [Description("Ver estadísticas")]
    VerEstadisticas = 1048576,

    [Description("Pagos Mercado Pago")]
    PagosMercadoPago = 2097152,

    [Description("Ver Mercado Pago")]
    VerMercadoPago = 4194304,

    [Description("Administrar Mercado Pago")]
    AdministrarMercadoPago = 8388608,

    All = ~None
}
