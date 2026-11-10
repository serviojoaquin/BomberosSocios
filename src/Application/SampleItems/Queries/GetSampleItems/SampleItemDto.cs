using BaseApp.Domain.Entities;

namespace BaseApp.Application.SampleItems.Queries.GetSampleItems;

public class SampleItemDto
{
    public int Id { get; init; }

    public string Description { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<SampleItem, SampleItemDto>();
        }
    }
}
