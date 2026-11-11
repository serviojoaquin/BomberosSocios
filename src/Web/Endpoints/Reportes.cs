using BaseApp.Application.Reportes.Commands.SaveCuotaSocio;
using BaseApp.Application.Reportes.Queries.GetReporteCuotasSocio;
using BaseApp.Application.Reportes.Queries.GetReporteCuotasSocioByCobrador;
using BaseApp.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BaseApp.Web.Endpoints;

public class Reportes : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization().RequirePermission(Permissions.VerCuotas);

        groupBuilder.MapPost(SaveCuotaSocio, "cuotas-socio");
        groupBuilder.MapGet(GetReporteCuotasSocio, "cuotas-socio/{socioId}/{year}").Produces<Stream>(StatusCodes.Status200OK, "application/pdf");
        groupBuilder.MapGet(GetReporteCuotasSocioByCobrador, "cuotas-socio-by-cobrador/{cobradorId}/{year}").Produces<Stream>(StatusCodes.Status200OK, "application/pdf");
    }

    [EndpointSummary("Guardar el reporte de cuotas de un socio")]
    [EndpointDescription("Guarda (o reemplaza) el PDF de cuotas del socio para el año indicado.")]
    public static async Task<Ok<SaveCuotaSocioResponse>> SaveCuotaSocio(ISender sender, SaveCuotaSocioCommand command)
    {
        var response = await sender.Send(command);

        return TypedResults.Ok(response);
    }

    [EndpointSummary("Descargar el reporte de cuotas de un socio")]
    [EndpointDescription("Devuelve el PDF de cuotas del socio para el año indicado; vacío si todavía no fue generado.")]
    public static async Task<FileContentHttpResult> GetReporteCuotasSocio(ISender sender, int socioId, int year)
    {
        var vm = await sender.Send(new GetReporteCuotasSocioQuery(socioId, year));

        return TypedResults.File(vm.Content, vm.ContentType, vm.FileName);
    }

    [EndpointSummary("Descargar los reportes de cuotas de los socios de un cobrador")]
    [EndpointDescription("Devuelve un único PDF con los reportes de cuotas de los socios activos del cobrador para el año indicado.")]
    public static async Task<FileContentHttpResult> GetReporteCuotasSocioByCobrador(ISender sender, int cobradorId, int year)
    {
        var vm = await sender.Send(new GetReporteCuotasSocioByCobradorQuery(cobradorId, year));

        return TypedResults.File(vm.Content, vm.ContentType, vm.FileName);
    }
}
