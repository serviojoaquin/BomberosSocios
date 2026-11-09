using System.ComponentModel;

namespace BaseApp.Domain.Enums;

[Flags]
public enum Permissions
{
    None = 0,

    [Description("View categories")]
    ViewCategories = 1,

    [Description("Create categories")]
    CreateCategories = 2,

    [Description("Edit categories")]
    EditCategories = 4,

    [Description("Delete categories")]
    DeleteCategories = 8,

    [Description("Manage security")]
    ManageSecurity = 16,

    All = ~None
}
