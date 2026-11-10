using BaseApp.Application.SampleItems.Commands.CreateSampleItem;
using BaseApp.Application.SampleItems.Commands.DeleteSampleItem;
using BaseApp.Application.SampleItems.Commands.UpdateSampleItem;
using BaseApp.Application.SampleItems.Queries.GetSampleItems;
using BaseApp.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BaseApp.Web.Endpoints;

public class SampleItems : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetSampleItems).RequirePermission(Permissions.ViewSampleItems);
        groupBuilder.MapPost(CreateSampleItem).RequirePermission(Permissions.CreateSampleItems);
        groupBuilder.MapPut(UpdateSampleItem, "{id}").RequirePermission(Permissions.EditSampleItems);
        groupBuilder.MapDelete(DeleteSampleItem, "{id}").RequirePermission(Permissions.DeleteSampleItems);
    }

    [EndpointSummary("Get all SampleItems")]
    [EndpointDescription("Retrieves all sampleItems ordered by description.")]
    public static async Task<Ok<SampleItemsVm>> GetSampleItems(ISender sender)
    {
        var vm = await sender.Send(new GetSampleItemsQuery());

        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Create a new SampleItem")]
    [EndpointDescription("Creates a new sampleItem and returns the ID of the created sampleItem.")]
    public static async Task<Created<int>> CreateSampleItem(ISender sender, CreateSampleItemCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(SampleItems)}/{id}", id);
    }

    [EndpointSummary("Update a SampleItem")]
    [EndpointDescription("Updates the specified sampleItem. The ID in the URL must match the ID in the payload.")]
    public static async Task<Results<NoContent, BadRequest>> UpdateSampleItem(ISender sender, int id, UpdateSampleItemCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a SampleItem")]
    [EndpointDescription("Deletes the sampleItem with the specified ID.")]
    public static async Task<NoContent> DeleteSampleItem(ISender sender, int id)
    {
        await sender.Send(new DeleteSampleItemCommand(id));

        return TypedResults.NoContent();
    }
}
