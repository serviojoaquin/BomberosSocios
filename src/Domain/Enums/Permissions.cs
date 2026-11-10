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

    All = ~None
}
