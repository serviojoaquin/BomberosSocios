namespace BaseApp.Application.SampleItems.Queries.GetSampleItems;

public class SampleItemsVm
{
    public IReadOnlyCollection<SampleItemDto> SampleItems { get; init; } = [];
}
