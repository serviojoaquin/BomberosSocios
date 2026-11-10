namespace BaseApp.Domain.Entities;

public class SampleItem : BaseAuditableEntity
{
    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}
