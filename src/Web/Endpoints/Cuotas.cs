using BaseApp.Application.Cuotas.Commands.BonificarCuotas;
using BaseApp.Application.Cuotas.Commands.BonificarCuotasViejas;
using BaseApp.Application.Cuotas.Commands.CobrarCuotas;
using BaseApp.Application.Cuotas.Commands.GenerarCuotas;
using BaseApp.Application.Cuotas.Commands.MarcarCuotaImpaga;
using BaseApp.Application.Cuotas.Dtos;
using BaseApp.Application.Cuotas.Queries.GetCuotasSocio;
using BaseApp.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BaseApp.Web.Endpoints;

public class Cuotas : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetCuotasSocio, "socio/{socioId}").RequirePermission(Permissions.VerCuotas);
        groupBuilder.MapPost(Generate, "generate").RequirePermission(Permissions.AdministrarCuotas);
        groupBuilder.MapPost(SetOldCuotasAsBonificada, "set-old-cuotas-as-bonificada").RequirePermission(Permissions.AdministrarCuotas);
        groupBuilder.MapPost(SetCuotasAsCobrada, "set-cuotas-as-cobrada").RequirePermission(Permissions.PagarCuotas);
        groupBuilder.MapPost(SetCuotasAsBonificada, "set-cuotas-as-bonificada").RequirePermission(Permissions.PagarCuotas);
        groupBuilder.MapPost(SetCuotaAsImpaga, "set-cuota-as-impaga").RequirePermission(Permissions.PagarCuotas);
    }

    [EndpointSummary("Cuotas de un socio")]
    [EndpointDescription("Devuelve el socio y todas sus cuotas, ordenadas por año descendente y mes.")]
    public static async Task<Ok<CuotasSocioVm>> GetCuotasSocio(ISender sender, int socioId)
    {
        var vm = await sender.Send(new GetCuotasSocioQuery(socioId));

        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Generar cuotas de un año")]
    [EndpointDescription("Crea las cuotas impagas del año para los socios activos (todos, de un cobrador o uno solo) y genera el reporte de cuotas de cada socio. Devuelve la cantidad creada.")]
    public static async Task<Ok<int>> Generate(ISender sender, GenerarCuotasCommand command)
    {
        var creadas = await sender.Send(command);

        return TypedResults.Ok(creadas);
    }

    [EndpointSummary("Bonificar cuotas impagas de años anteriores")]
    [EndpointDescription("Marca como bonificadas las cuotas impagas de años anteriores al actual, de un socio o de los socios de un cobrador.")]
    public static async Task<Ok<ChangeStatusCuotaResponse>> SetOldCuotasAsBonificada(ISender sender, BonificarCuotasViejasCommand command)
    {
        var response = await sender.Send(command);

        return TypedResults.Ok(response);
    }

    [EndpointSummary("Cobrar cuotas")]
    [EndpointDescription("Marca las cuotas indicadas como cobradas con la fecha actual y el cobrador del socio.")]
    public static async Task<Ok<ChangeStatusCuotaResponse>> SetCuotasAsCobrada(ISender sender, CobrarCuotasCommand command)
    {
        var response = await sender.Send(command);

        return TypedResults.Ok(response);
    }

    [EndpointSummary("Bonificar cuotas")]
    [EndpointDescription("Marca las cuotas indicadas como bonificadas con la fecha actual.")]
    public static async Task<Ok<ChangeStatusCuotaResponse>> SetCuotasAsBonificada(ISender sender, BonificarCuotasCommand command)
    {
        var response = await sender.Send(command);

        return TypedResults.Ok(response);
    }

    [EndpointSummary("Marcar una cuota como impaga")]
    [EndpointDescription("Deshace el cobro o la bonificación de la cuota indicada.")]
    public static async Task<Ok<ChangeStatusCuotaResponse>> SetCuotaAsImpaga(ISender sender, MarcarCuotaImpagaCommand command)
    {
        var response = await sender.Send(command);

        return TypedResults.Ok(response);
    }
}
