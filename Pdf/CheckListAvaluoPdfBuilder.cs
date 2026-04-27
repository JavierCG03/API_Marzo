using CarSlineAPI.Models.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CarSlineAPI.Pdf
{
    /// <summary>
    /// Construye el documento PDF de Check List de Toma de Autos.
    /// Secciones: encabezado, datos del vehículo/vendedor, accesorios,
    /// llantas/batería, combustible/kilometraje, observaciones y firmas.
    /// </summary>
    public class CheckListAvaluoPdfBuilder : IPdfDocumentBuilder
    {
        private readonly CheckListAvaluoCompletoPdf _checkList;
        private readonly string _logoPath;

        private static readonly string RojoOscuro = Colors.Red.Darken2;
        private static readonly string GrisClaro = Colors.Grey.Lighten2;
        private static readonly string GrisLighten3 = Colors.Grey.Lighten3;

        public CheckListAvaluoPdfBuilder(CheckListAvaluoCompletoPdf checkList)
        {
            _checkList = checkList;
            _logoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "logo.png");
        }

        // -------------------------------------------------------
        // Punto de entrada
        // -------------------------------------------------------

        public IDocument Build()
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.MarginTop(15);
                    page.MarginRight(40);
                    page.MarginBottom(25);
                    page.MarginLeft(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(CrearEncabezado);
                    page.Content().Element(CrearContenido);
                    page.Footer().Element(CrearPiePagina);
                });
            });
        }

        // -------------------------------------------------------
        // ENCABEZADO
        // -------------------------------------------------------

        private void CrearEncabezado(IContainer container)
        {
            string folio = $"Folio Avaluo: {_checkList.Avaluo.Id:D6}";

            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Height(35).Image(_logoPath).FitArea();
                        col.Item().Text("📍 Las Palomas 590, El Portezuelo  ☎ Tel:\u00A0771-295-4232")
                            .FontSize(10).Italic();
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(folio)
                            .AlignRight().FontSize(11).Bold().FontColor(Colors.Grey.Darken2);
                        col.Item().PaddingTop(2)
                            .Text("CHECK LIST")
                            .AlignCenter().FontSize(15).Bold().FontColor(RojoOscuro).Italic();
                        col.Item().PaddingTop(1)
                            .Text("TOMA DE AUTOS")
                            .AlignCenter().FontSize(12).Bold().FontColor(RojoOscuro);
                    });
                });

                column.Item().PaddingTop(5).LineHorizontal(2).LineColor(RojoOscuro);
            });
        }

        // -------------------------------------------------------
        // CONTENIDO PRINCIPAL
        // -------------------------------------------------------

        private void CrearContenido(IContainer container)
        {
            container.PaddingTop(5).Column(column =>
            {
                // 1. Datos del vehículo y vendedor
                column.Item().Element(SeccionVehiculoVendedor);

                // 2. Kilometraje y combustible
                column.Item().PaddingTop(6).Element(SeccionKilometrajeCombustible);

                // 3. Accesorios — tabla principal de ítems
                column.Item().PaddingTop(6).Element(SeccionAccesorios);

                // 4. Llantas y batería
                column.Item().PaddingTop(6).Element(SeccionLlantasBateria);

                // 5. Observaciones / comentarios
                if (_checkList.CheckList != null &&
                    (!string.IsNullOrWhiteSpace(_checkList.CheckList.Comentarios) ||
                     !string.IsNullOrWhiteSpace(_checkList.CheckList.Observaciones)))
                {
                    column.Item().PaddingTop(6).Element(SeccionObservaciones);
                }

                // 6. Firmas — empujado al fondo de la página
                column.Item().ExtendVertical().AlignBottom().Element(SeccionFirmas);
            });
        }

        // -------------------------------------------------------
        // SECCIÓN 1 — Vehículo y vendedor
        // -------------------------------------------------------

        private void SeccionVehiculoVendedor(IContainer container)
        {
            var a = _checkList.Avaluo;
            var c = _checkList.CheckList;

            container.Border(1).BorderColor(GrisClaro).Padding(6).Column(col =>
            {
                col.Item().Text("VEHÍCULO").FontSize(9).Bold().FontColor(RojoOscuro).AlignCenter();

                // Fila 1: Marca / Modelo / Versión / Color
                col.Item().PaddingTop(3).Row(row =>
                {
                    CeldaInfo(row.RelativeItem(), "Marca:", a.Marca);
                    CeldaInfo(row.RelativeItem(), "Modelo:", a.Modelo);
                    CeldaInfo(row.RelativeItem(), "Versión:", a.Version);
                    CeldaInfo(row.RelativeItem(), "Color:", a.Color ?? "—");
                });

                // Fila 2: Año / VIN / Placas / (vacío)
                col.Item().PaddingTop(4).Row(row =>
                {
                    CeldaInfo(row.RelativeItem(), "Año:", a.Anio.ToString());
                    CeldaInfo(row.RelativeItem(), "VIN:", a.VIN);

                    row.RelativeItem(2).Text(txt =>
                    {
                        var label = string.IsNullOrWhiteSpace(a.PlacasEdo)
                            ? "Placas:"
                            : $"Placas {a.PlacasEdo}:";
                        txt.Span(label).FontSize(8).FontColor(Colors.Grey.Darken2);
                        txt.Span(" " + (string.IsNullOrWhiteSpace(a.Placas) ? "S/P" : a.Placas))
                            .FontSize(8).Bold();
                    });
                });

                // Fila 3: Vendedor y teléfonos
                col.Item().PaddingTop(4).Row(row =>
                {
                    row.ConstantItem(220).Text(txt =>
                    {
                        txt.Span("Vendedor: ").FontSize(9).FontColor(Colors.Grey.Darken2);
                        txt.Span(a.NombreCompleto).FontSize(9).Bold();
                    });

                    row.ConstantItem(90).Text(txt =>
                    {
                        txt.Span("Tel: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                        txt.Span(a.Telefono1).FontSize(9).Bold();
                    });

                    if (!string.IsNullOrWhiteSpace(a.Telefono2))
                    {
                        row.ConstantItem(90).Text(txt =>
                        {
                            txt.Span("Tel 2: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                            txt.Span(a.Telefono2).FontSize(9).Bold();
                        });
                    }
                });

            });
        }

        // -------------------------------------------------------
        // SECCIÓN 2 — Kilometraje y combustible
        // -------------------------------------------------------

        private void SeccionKilometrajeCombustible(IContainer container)
        {
            var c = _checkList.CheckList;
            if (c == null) return;

            container.Row(row =>
            {
                // Kilometraje
                row.RelativeItem().Border(1).BorderColor(GrisClaro).Padding(6).Column(col =>
                {
                    col.Item().Text("KILOMETRAJE").FontSize(7).Bold()
                        .FontColor(RojoOscuro).AlignCenter();
                    col.Item().PaddingTop(4).Text($"{c.Kilometraje:N0} km")
                        .FontSize(12).Bold().AlignCenter();
                });

                row.ConstantItem(10);

                // Combustible — barra gráfica
                row.RelativeItem().Border(1).BorderColor(GrisClaro).Padding(4).Column(col =>
                {
                    col.Item().Text("NIVEL DE COMBUSTIBLE").FontSize(7).Bold()
                        .FontColor(RojoOscuro).AlignCenter();

                    col.Item().PaddingTop(2).Row(barRow =>
                    {
                        // 8 divisiones (0 = vacío … 8 = lleno)
                        byte nivel = c.Combustible ?? 0;
                        for (int i = 1; i <= 8; i++)
                        {
                            string bgColor = i <= nivel
                                ? (nivel <= 2 ? Colors.Red.Medium
                                 : nivel <= 5 ? Colors.Orange.Medium
                                 : Colors.Green.Medium)
                                : GrisClaro;

                            barRow.RelativeItem()
                                .Height(12)
                                .Background(bgColor)
                                .Border(0.5f).BorderColor(Colors.White);
                        }
                    });

                    // Etiquetas E / 1/2 / F
                    col.Item().PaddingTop(2).Row(lblRow =>
                    {
                        lblRow.RelativeItem().Text("E").FontSize(7).AlignLeft();
                        lblRow.RelativeItem().Text("1/2").FontSize(7).AlignCenter();
                        lblRow.RelativeItem().Text("F").FontSize(7).AlignRight();
                    });
                });
            });
        }

        // -------------------------------------------------------
        // SECCIÓN 3 — Accesorios (checklist de ítems booleanos)
        // -------------------------------------------------------

        private void SeccionAccesorios(IContainer container)
        {
            var c = _checkList.CheckList;
            if (c == null) return;

            // Definición de todos los ítems agrupados
            var grupos = new[]
            {
                ("HERRAMIENTA", new (string, bool)[]
                {
                    ("Gato",                c.Gato),
                    ("Llave cruz / L",   c.LlaveLoCruz),
                    ("Gancho de arrastre",  c.GanchoArrastre),
                    ("Maneral",             c.Maneral),
                    ("Varillas (Pick Up)",  c.VarillasPickUp),
                    ("Batería",             c.Bateria),
                    ("Llanta Refacción",    c.Refaccion),
                }),
                ("LLAVE / SEGURIDAD", new (string, bool)[]
                {
                    ("Duplicado de llave",  c.DuplicadoLlave),
                    ("Birlo de seguridad",  c.BirloSeguridad),
                    ("Candado de seguridad",c.CandadoSeguridad),
                }),
                ("EXTERIOR", new (string, bool)[]
                {

                    
                    ("Rines",               c.Rines),
                    ("Tapones de rin",             c.Tapones),
                    ("Centro de rin",       c.CentroRin),
                    ("Faros de niebla",     c.FarosNiebla),
                    ("Limpiador delantero", c.LimpiadorDelantero),
                    ("Limpiador trasero",   c.LimpiadorTrasero),
                    ("Tapon de Gasolina",   c.TaponGasolina),
                    ("Antena de radio",        c.AntenaRadio),

                }),
                ("INTERIOR", new (string, bool)[]
                {
                    ("Radio / Estéreo",     c.RadioEstereo),
                    ("Viseras",             c.Viseras),
                    ("Cabeceras",           c.Cabeceras),
                    ("Tapetes",             c.Tapetes),
                    ("Encendedor",          c.Encendedor),
                    ("Maletín",             c.Maletin),
                    ("Cubresala",           c.Cubresala),
                    ("Cubrevolante",        c.Cubrevolante),
                    ("Tapa cajuela",        c.TapaCajuela),
                    ("Cortina cajuela",     c.CortinaCajuela),
                }),
            };

            container.Column(column =>
            {
                column.Item().Background(RojoOscuro).Padding(3)
                    .Text("ACCESORIOS / CHECKLIST").FontSize(9).Bold().FontColor(Colors.White);

                column.Item().PaddingTop(4).Table(table =>
                {
                    // 4 columnas: nombre | ✓✗  | nombre | ✓✗
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3);
                        cols.ConstantColumn(22);
                        cols.RelativeColumn(3);
                        cols.ConstantColumn(22);
                        cols.RelativeColumn(3);
                        cols.ConstantColumn(22);
                        cols.RelativeColumn(3);
                        cols.ConstantColumn(22);
                    });

                    foreach (var (titulo, items) in grupos)
                    {
                        // Encabezado del grupo — ocupa las 4 columnas
                        table.Cell().ColumnSpan(8)
                            .Background(GrisLighten3).Padding(3)
                            .Text(titulo).FontSize(8).Bold().FontColor(Colors.Grey.Darken3);

                        // Filas de ítems en dos columnas
                        for (int i = 0; i < items.Length; i += 4)
                        {
                            FilaCheckItem(table, items[i].Item1, items[i].Item2);

                            if (i + 1 < items.Length)
                                FilaCheckItem(table, items[i + 1].Item1, items[i + 1].Item2);
                            else
                            {
                                table.Cell().BorderBottom(1).BorderColor(GrisClaro).Padding(3);
                                table.Cell().BorderBottom(1).BorderColor(GrisClaro).Padding(3);
                            }

                            if (i + 2 < items.Length)
                                FilaCheckItem(table, items[i + 2].Item1, items[i + 2].Item2);
                            else
                            {
                                table.Cell().BorderBottom(1).BorderColor(GrisClaro).Padding(3);
                                table.Cell().BorderBottom(1).BorderColor(GrisClaro).Padding(3);
                            }

                            if (i + 3 < items.Length)
                                FilaCheckItem(table, items[i + 3].Item1, items[i + 3].Item2);
                            else
                            {
                                table.Cell().BorderBottom(1).BorderColor(GrisClaro).Padding(3);
                                table.Cell().BorderBottom(1).BorderColor(GrisClaro).Padding(3);
                            }
                        }
                    }
                });
            });
        }

        // -------------------------------------------------------
        // SECCIÓN 4 — Llantas y batería
        // -------------------------------------------------------

        private void SeccionLlantasBateria(IContainer container)
        {
            var c = _checkList.CheckList;
            if (c == null) return;

            container.Column(column =>
            {
                column.Item().Background(RojoOscuro).Padding(3)
                    .Text("LLANTAS Y BATERÍA").FontSize(9).Bold().FontColor(Colors.White);

                column.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                    });

                    // Encabezados
                    foreach (var h in new[] { "DELANTERA DER.", "DELANTERA IZQ.", "TRASERA DER.", "TRASERA IZQ." })
                    {
                        table.Cell().Background(GrisLighten3).Padding(3)
                            .Text(h).FontSize(8).Bold().AlignCenter();
                    }

                    CeldaLlantaCompleta(table, c.MarcaLlantaDelanteraDer, c.MedidaLlantaDelanteraDer);
                    CeldaLlantaCompleta(table, c.MarcaLlantaDelanteraIzq, c.MedidaLlantaDelanteraIzq);
                    CeldaLlantaCompleta(table, c.MarcaLlantaTraseraDer, c.MedidaLlantaTraseraDer);
                    CeldaLlantaCompleta(table, c.MarcaLlantaTraseraIzq, c.MedidaLlantaTraseraIzq);
                });

                // Refacción y batería en la misma fila
                column.Item().PaddingTop(5).Row(row =>
                {
                    // Llanta de refacción
                    row.RelativeItem().Border(1).BorderColor(GrisClaro).Padding(5).Column(col =>
                    {
                        col.Item().Text("LLANTA DE REFACCIÓN").FontSize(8).Bold()
                            .FontColor(RojoOscuro).AlignCenter();
                        col.Item().PaddingTop(3).Row(r =>
                        {
                            r.RelativeItem().Text(txt =>
                            {
                                txt.Span("Marca: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                                txt.Span(c.MarcaLlantaRefaccion ?? "—").FontSize(8).Bold();
                            });
                            r.RelativeItem().Text(txt =>
                            {
                                txt.Span("Medida: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                                txt.Span(c.MedidaLlantaRefaccion ?? "—").FontSize(8).Bold();
                            });
                        });
                    });

                    row.ConstantItem(10);

                    // Batería
                    row.RelativeItem().Border(1).BorderColor(GrisClaro).Padding(5).Column(col =>
                    {
                        col.Item().Text("BATERÍA").FontSize(8).Bold()
                            .FontColor(RojoOscuro).AlignCenter();
                        col.Item().PaddingTop(3).Text(txt =>
                        {
                            txt.Span("Marca: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                            txt.Span(c.MarcaBateria ?? "—").FontSize(8).Bold();
                        });
                    });
                });
            });
        }

        // -------------------------------------------------------
        // SECCIÓN 5 — Observaciones
        // -------------------------------------------------------

        private void SeccionObservaciones(IContainer container)
        {
            var c = _checkList.CheckList;
            if (c == null) return;

            container.Column(column =>
            {
                column.Item().Background(RojoOscuro).Padding(3)
                    .Text("OBSERVACIONES").FontSize(9).Bold().FontColor(Colors.White);

                if (!string.IsNullOrWhiteSpace(c.Comentarios))
                {
                    column.Item().PaddingTop(4).Border(1).BorderColor(GrisClaro).Padding(5).Column(col =>
                    {
                        col.Item().Text("Comentarios:").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                        col.Item().PaddingTop(2).Text(c.Comentarios).FontSize(9);
                    });
                }

                if (!string.IsNullOrWhiteSpace(c.Observaciones))
                {
                    column.Item().PaddingTop(4).Border(1).BorderColor(GrisClaro).Padding(5).Column(col =>
                    {
                        col.Item().Text("Observaciones:").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                        col.Item().PaddingTop(2).Text(c.Observaciones).FontSize(9);
                    });
                }
            });
        }

        // -------------------------------------------------------
        // SECCIÓN 6 — Firmas
        // -------------------------------------------------------
        private void SeccionFirmas(IContainer container)
        {
            container.PaddingTop(10).AlignCenter().Row(row =>
            {
                row.ConstantItem(180).Element(x =>
                    LineaFirma(x, _checkList.Avaluo.AsesorNombre, "Asesor")
                );

                row.ConstantItem(40); // espacio entre firmas

                row.ConstantItem(180).Element(x =>
                    LineaFirma(x, _checkList.CheckList.VigilanteNombre, "Vigilante")
                );
            });
        }
        // -------------------------------------------------------
        // PIE DE PÁGINA
        // -------------------------------------------------------

        private void CrearPiePagina(IContainer container)
        {
            container.AlignBottom().Column(column =>
            {
                column.Item().LineHorizontal(1).LineColor(GrisClaro);

                column.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text(txt =>
                    {
                        txt.Span("Generado: ").FontSize(8);
                        txt.Span(DateTime.Now.ToString("dd/MMM/yyyy HH:mm")).FontSize(8).Bold();
                    });

                    row.ConstantItem(120).AlignRight().Text(txt =>
                    {
                        txt.Span("Página ").FontSize(8);
                        txt.CurrentPageNumber().FontSize(8).Bold();
                        txt.Span(" de ").FontSize(8);
                        txt.TotalPages().FontSize(8).Bold();
                    });
                });
            });
        }

        // -------------------------------------------------------
        // HELPERS
        // -------------------------------------------------------

        /// <summary>Celda de info estilo label: valor.</summary>
        private static void CeldaInfo(IContainer slot, string label, string valor)
        {
            slot.Text(txt =>
            {
                txt.Span(label + " ").FontSize(8).FontColor(Colors.Grey.Darken2);
                txt.Span(valor).FontSize(8).Bold();
            });
        }

        /// <summary>Par de celdas nombre | ✓✗ para la tabla de 4 columnas.</summary>
        private static void FilaCheckItem(TableDescriptor table, string nombre, bool valor)
        {
            table.Cell().BorderBottom(1).BorderColor(GrisClaro).Padding(3)
                .Text(nombre).FontSize(8);

            table.Cell().BorderBottom(1).BorderColor(GrisClaro).AlignCenter().Padding(2)
                .Text(valor ? "✓" : "✗").FontSize(9).Bold()
                .FontColor(valor ? Colors.Green.Darken2 : Colors.Red.Medium);
        }

        private static void CeldaLlantaCompleta(TableDescriptor table, string? marca, string? medida)
        {
            table.Cell()
                .AlignCenter()
                .Padding(3)
                .Text(txt =>
                {
                    // Marca (más visible)
                    txt.Span(string.IsNullOrWhiteSpace(marca) ? "—" : marca)
                       .FontSize(8)
                       .Bold();

                    // Separador
                    txt.Span(" ");

                    // Medida (más ligera)
                    txt.Span(string.IsNullOrWhiteSpace(medida) ? "" : $"( {medida} )")
                       .FontSize(7.5f)
                       .FontColor(Colors.Grey.Darken2);
                });
        }

        /// <summary>Línea de firma con etiqueta debajo.</summary>
        private void LineaFirma(IContainer container, string nombre, string puesto)
        {
            container.Column(col =>
            {
                // Línea de firma
                col.Item().PaddingTop(25)
                    .LineHorizontal(1)
                    .LineColor(Colors.Grey.Darken1);

                // Nombre
                if (!string.IsNullOrWhiteSpace(nombre))
                    col.Item().PaddingTop(4)
                        .Text(nombre)
                        .FontSize(9)
                        .Bold()
                        .AlignCenter();

                // Puesto
                if (!string.IsNullOrWhiteSpace(puesto))
                    col.Item()
                        .Text(puesto)
                        .FontSize(8)
                        .FontColor(Colors.Grey.Darken2)
                        .AlignCenter();
            });
        }
    }
}