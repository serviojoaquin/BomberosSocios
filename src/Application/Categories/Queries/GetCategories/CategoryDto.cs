using BaseApp.Domain.Entities;

namespace BaseApp.Application.Categories.Queries.GetCategories;

public class CategoryDto
{
    public int Id { get; init; }

    public string Description { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Category, CategoryDto>();
        }
    }
}
