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

    [Description("Manage security")]
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

    All = ~None
}
