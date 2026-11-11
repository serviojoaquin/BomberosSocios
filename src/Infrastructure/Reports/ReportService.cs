using System.Globalization;
using System.Reflection;
using BaseApp.Application.Common.Interfaces;
using BaseApp.Domain.Entities;
using BaseApp.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BaseApp.Infrastructure.Reports;

public class ReportService : IReportService
{
    private const string BomberosNombre = "Asociación de bomberos voluntarios de Bragado";
    private const string BomberosPersonaJuridica = "4326";
    private const string BomberosCuit = "30-66444242-2";
    private const int MaxLength = 25;

    private static readonly CultureInfo Cultura = new("es-AR");
    private static readonly byte[] Logo = CargarLogo();

    public byte[] GeneratePdfReportCuotasSocio(Socio socio, IReadOnlyCollection<Cuota> cuotas, int year)
    {
        var cobradorNombre = socio.Cobrador.NombreCompleto;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginVertical(10);
                page.MarginHorizontal(14);
                page.DefaultTextStyle(t => t.FontFamily(Fonts.Arial).FontSize(10));

                page.Content().Column(column =>
                {
                    column.Item().AlignCenter().Text(BomberosNombre.ToUpperInvariant()).Bold().FontSize(15);

                    column.Item().PaddingBottom(6).Row(row =>
                    {
                        row.RelativeItem().AlignCenter().Text(text =>
                        {
                            text.Span("Personería Jurídica: ").Bold().FontSize(9);
                            text.Span(BomberosPersonaJuridica).FontSize(9);
                        });
                        row.RelativeItem().AlignCenter().Text(text =>
                        {
                            text.Span("CUIT: ").Bold().FontSize(9);
                            text.Span(BomberosCuit).FontSize(9);
                        });
                    });

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text(text => Dato(text, "Número de Socio: ", socio.Id.ToString()));
                        row.RelativeItem(2).Text(text => Dato(text, "Nombre y/o Razón Social: ", socio.Nombre));
                    });
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text(text => Dato(text, "Cobrador: ", cobradorNombre));
                        row.RelativeItem().Text(text => Dato(text, "Fecha de Ingreso: ", socio.FechaAfiliacion.Year > 2000 ? socio.FechaAfiliacion.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty));
                        row.RelativeItem().Text(text => Dato(text, "Teléfono: ", socio.Telefono));
                    });
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text(text => Dato(text, "Domicilio: ", socio.Domicilio));
                        row.RelativeItem().Text(text => Dato(text, "Domicilio de cobro: ", socio.DomicilioDeCobro));
                    });

                    column.Item().PaddingTop(12).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        for (var mes = 12; mes >= 1; mes--)
                        {
                            var cuota = cuotas.FirstOrDefault(c => (int)c.Mes == mes);
                            var mesNombre = new DateTime(year, mes, 1).ToString("MMMM", Cultura);
                            var cobradorCuota = cuota?.Cobrador is not null ? cuota.Cobrador.NombreCompleto : cobradorNombre;

                            table.Cell().PaddingHorizontal(9).PaddingBottom(14).Column(card =>
                            {
                                card.Item().Row(row =>
                                {
                                    row.ConstantItem(52).Image(Logo).FitWidth();
                                    row.RelativeItem().AlignRight().AlignBottom().Column(c =>
                                    {
                                        c.Item().AlignRight().Text("N° de Socio").FontColor(Colors.Grey.Medium).FontSize(11);
                                        c.Item().AlignRight().Text(socio.Id.ToString()).FontSize(12);
                                    });
                                });
                                Etiqueta(card, "Nombre y/o Razon Social");
                                Valor(card, Cortar(socio.Nombre));
                                Etiqueta(card, "Mes y Año");
                                Valor(card, $"{mesNombre} de {year}");
                                card.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(c =>
                                    {
                                        Etiqueta(c, "Importe");
                                        Valor(c, cuota is not null ? $"${cuota.Importe}" : "0");
                                    });
                                    row.RelativeItem().AlignRight().Column(c =>
                                    {
                                        c.Item().AlignRight().Text("Estado").FontColor(Colors.Grey.Medium).FontSize(11);
                                        c.Item().AlignRight().Text((cuota?.Estado ?? CuotaEstado.Invalida).ToString().ToUpperInvariant()).FontSize(12);
                                    });
                                });
                                Etiqueta(card, "Cobrador");
                                Valor(card, Cortar(cobradorCuota));
                            });
                        }
                    });
                });
            });
        }).GeneratePdf();
    }

    private static void Dato(TextDescriptor text, string etiqueta, string? valor)
    {
        text.Span(etiqueta).Bold().FontSize(11);
        text.Span((valor ?? string.Empty).ToUpperInvariant()).FontSize(11);
    }

    private static void Etiqueta(ColumnDescriptor column, string texto)
    {
        column.Item().PaddingTop(3).Text(texto).FontColor(Colors.Grey.Medium).FontSize(11);
    }

    private static void Valor(ColumnDescriptor column, string texto)
    {
        column.Item().Text(texto.ToUpperInvariant()).FontSize(12);
    }

    private static string Cortar(string texto)
    {
        return texto.Length > MaxLength ? texto[..(MaxLength - 1)] + "..." : texto;
    }

    private static byte[] CargarLogo()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BaseApp.Infrastructure.Reports.logo.png")
            ?? throw new InvalidOperationException("No se encontró el logo embebido para los reportes.");
        using var memoria = new MemoryStream();
        stream.CopyTo(memoria);

        return memoria.ToArray();
    }
}
