/*
using CarSlineAPI.Models.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CarSlineAPI.Pdf
{
    /// <summary>
    /// Construye el documento PDF de Avalúos de Vehículos.
    /// Agrega aquí todas las secciones propias del avalúo sin tocar
    /// OrdenPdfBuilder ni PdfService.
    /// </summary>
    public class AvaluoPdfBuilder : IPdfDocumentBuilder
    {
        private readonly AvaluoPdfDto _avaluo;
        private readonly string _logoPath;

        public AvaluoPdfBuilder(AvaluoPdfDto avaluo)
        {
            _avaluo = avaluo;
            _logoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "logo.png");
        }

        // -------------------------------------------------------
        // Punto de entrada — requerido por IPdfDocumentBuilder
        // -------------------------------------------------------

        public IDocument Build()
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.MarginTop(25);
                    page.MarginRight(40);
                    page.MarginBottom(30);
                    page.MarginLeft(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(c => CrearEncabezado(c));
                    page.Content().Element(c => CrearContenido(c));
                    page.Footer().Element(c => CrearPiePagina(c));
                });
            });
        }

        // -------------------------------------------------------
        // Secciones principales
        // -------------------------------------------------------

        private void CrearEncabezado(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Height(40).Image(_logoPath).FitArea();
                        col.Item().Text("📍 Las Palomas 590, El Portezuelo").FontSize(10).Italic();
                        col.Item().Text(" ☎ Tel: 771-295-4232").FontSize(9);
                    });

                    row.ConstantItem(150).Column(col =>
                    {
                        col.Item().Background(Colors.Blue.Darken2).Padding(8).Column(c =>
                        {
                            c.Item().Text(_avaluo.NumeroAvaluo).AlignRight()
                                .FontSize(16).Bold().FontColor(Colors.White);
                            c.Item().Text("AVALÚO").AlignRight()
                                .FontSize(11).FontColor(Colors.White);
                        });

                        col.Item().Padding(5).Column(c =>
                        {
                            c.Item().Text($"Fecha: {_avaluo.FechaAvaluo:dd/MMM/yyyy}").FontSize(9).Bold();
                        });
                    });
                });

                column.Item().PaddingTop(5).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
            });
        }

        private void CrearContenido(IContainer container)
        {
            container.PaddingTop(15).Column(column =>
            {
                // TODO: Agrega aquí tus secciones de avalúo, por ejemplo:
                // column.Item().Element(c => SeccionDatosVehiculo(c));
                // column.Item().PaddingTop(10).Element(c => SeccionInspeccionExterior(c));
                // column.Item().PaddingTop(10).Element(c => SeccionInspeccionMecanica(c));
                // column.Item().PaddingTop(10).Element(c => SeccionValoracion(c));

                column.Item().Text("[ Contenido del avalúo — implementar secciones ]")
                    .FontSize(11).Italic().FontColor(Colors.Grey.Medium);
            });
        }

        private void CrearPiePagina(IContainer container)
        {
            container.AlignBottom().Column(column =>
            {
                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Text(txt =>
                    {
                        txt.Span("Generado: ").FontSize(8);
                        txt.Span(DateTime.Now.ToString("dd/MMM/yyyy HH:mm")).FontSize(8).Bold();
                    });

                    row.ConstantItem(100).AlignRight().Text(txt =>
                    {
                        txt.Span("Página ").FontSize(8);
                        txt.CurrentPageNumber().FontSize(8).Bold();
                        txt.Span(" de ").FontSize(8);
                        txt.TotalPages().FontSize(8).Bold();
                    });
                });
            });
        }
    }
}*/