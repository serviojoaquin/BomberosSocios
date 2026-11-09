using BaseApp.Application.Categories.Commands.CreateCategory;
using BaseApp.Application.Categories.Commands.DeleteCategory;
using BaseApp.Application.Categories.Commands.UpdateCategory;
using BaseApp.Application.Categories.Queries.GetCategories;
using BaseApp.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BaseApp.Web.Endpoints;

public class Categories : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetCategories).RequirePermission(Permissions.ViewCategories);
        groupBuilder.MapPost(CreateCategory).RequirePermission(Permissions.CreateCategories);
        groupBuilder.MapPut(UpdateCategory, "{id}").RequirePermission(Permissions.EditCategories);
        groupBuilder.MapDelete(DeleteCategory, "{id}").RequirePermission(Permissions.DeleteCategories);
    }

    [EndpointSummary("Get all Categories")]
    [EndpointDescription("Retrieves all categories ordered by description.")]
    public static async Task<Ok<CategoriesVm>> GetCategories(ISender sender)
    {
        var vm = await sender.Send(new GetCategoriesQuery());

        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Create a new Category")]
    [EndpointDescription("Creates a new category and returns the ID of the created category.")]
    public static async Task<Created<int>> CreateCategory(ISender sender, CreateCategoryCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Categories)}/{id}", id);
    }

    [EndpointSummary("Update a Category")]
    [EndpointDescription("Updates the specified category. The ID in the URL must match the ID in the payload.")]
    public static async Task<Results<NoContent, BadRequest>> UpdateCategory(ISender sender, int id, UpdateCategoryCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a Category")]
    [EndpointDescription("Deletes the category with the specified ID.")]
    public static async Task<NoContent> DeleteCategory(ISender sender, int id)
    {
        await sender.Send(new DeleteCategoryCommand(id));

        return TypedResults.NoContent();
    }
}
