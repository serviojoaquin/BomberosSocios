namespace BaseApp.Application.Categories.Queries.GetCategories;

public class CategoriesVm
{
    public IReadOnlyCollection<CategoryDto> Categories { get; init; } = [];
}
