using BaseApp.Application.Dashboard;
using BaseApp.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BaseApp.Web.Endpoints;

public class Dashboard : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization().RequirePermission(Permissions.VerEstadisticas);

        groupBuilder.MapGet(GetCobranzaCuotaSocios, "cobranza-cuota-socios/{year}");
    }

    [EndpointSummary("Cobranza de cuotas por cobrador")]
    [EndpointDescription("Para el año indicado, cantidad de socios activos por cobrador según cuántas cuotas tienen cobradas (0 a 12), con porcentajes y fila de totales al final.")]
    public static async Task<Ok<CobranzaCuotaSociosVm>> GetCobranzaCuotaSocios(ISender sender, int year)
    {
        var vm = await sender.Send(new CobranzaCuotaSociosQuery(year));

        return TypedResults.Ok(vm);
    }
}
