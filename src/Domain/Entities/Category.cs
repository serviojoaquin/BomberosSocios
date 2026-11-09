namespace BaseApp.Domain.Entities;

public class Category : BaseAuditableEntity
{
    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}
