using CarSlineAPI.Models.DTOs;
using CarSlineAPI.Models.Entities;
using Mysqlx.Crud;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CarSlineAPI.Pdf
{
    /// <summary>
    /// Construye el documento PDF de Avalúos de Vehículos.
    /// Recibe AvaluoCompletoResponse con Avaluo, Equipamiento y Reparaciones.
    /// </summary>
    public class AvaluoPdfBuilder : IPdfDocumentBuilder
    {
        private readonly AvaluoCompletoResponse _data;
        private readonly string _logoPath;

        private static readonly string RojoOscuro = Colors.Red.Darken2;
        private static readonly string GrisClaro = Colors.Grey.Lighten2;
        private static readonly string GrisLighten3 = Colors.Grey.Lighten3;

        public AvaluoPdfBuilder(AvaluoCompletoResponse data)
        {
            _data = data;
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
            var avaluo = _data.Avaluo!;
            string folio = $"Folio: {_data.AvaluoId:D6}";
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item()
                            .Height(35)
                            .Image(_logoPath)
                            .FitArea();
                        col.Item().Text("📍 Las Palomas 590, El Portezuelo  ☎ Tel:\u00A0771-295-4232").FontSize(10).Italic();
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(folio).AlignRight().FontSize(11).Bold().FontColor(Colors.Grey.Darken2);
                        col.Item().PaddingTop(5).Text("AVALÚO").AlignCenter().FontSize(25).Bold().FontColor(RojoOscuro).Italic();
                    });
                });

                column.Item().PaddingTop(5).LineHorizontal(2).LineColor(Colors.Red.Darken2);
            });
        }


        // -------------------------------------------------------
        // CONTENIDO PRINCIPAL
        // -------------------------------------------------------

        private void CrearContenido(IContainer container)
        {
            container.PaddingTop(5).Column(column =>
            {
                // 1. Datos del vendedor y vehículo
                column.Item().Element(SeccionVendedorVehiculo);

                // 2. Documentación
                column.Item().PaddingTop(6).Element(SeccionDocumentacion);

                // 3. Equipamiento (checklist)
                if (_data.Equipamiento != null)
                    column.Item().PaddingTop(6).Element(SeccionEquipamiento);

                // 4. Reparaciones
                if (_data.Reparaciones != null && _data.Reparaciones.Any())
                    column.Item().PaddingTop(6).Element(SeccionReparaciones);

                // 5. Costos
                column.Item().PaddingTop(6).Element(SeccionCostos);
                // Seccion 6 y 7 extendiadas hasta el final
                column.Item().ExtendVertical().AlignBottom().Column(col =>
                {
                    // 6. Notas legales
                    col.Item().Element(SeccionNotas);
                    // 7. Firmas
                    col.Item().PaddingTop(10).Element(SeccionFirmas);
                });
            });
        }

        // -------------------------------------------------------
        // SECCIÓN 1 — Vendedor y Vehículo (layout vertical compacto)
        // -------------------------------------------------------

        private void SeccionVendedorVehiculo(IContainer container)
        {
            var a = _data.Avaluo!;
            var e = _data.Equipamiento;

            container.Column(column =>
            {
                // --- VENDEDOR ---
                column.Item().PaddingTop(4).Border(1).BorderColor(GrisClaro).Padding(6).Column(col =>
                {
                    col.Item().Text("VENDEDOR").FontSize(9).Bold().FontColor(RojoOscuro).AlignCenter();
                    col.Item().PaddingTop(3).Row(row =>
                    {
                        // 📌 Nombre (más grande)
                        row.ConstantItem(190).Text(txt =>
                        {
                            txt.Span("Nombre: ").FontSize(9).FontColor(Colors.Grey.Darken2);
                            txt.Span(a.NombreCompleto).FontSize(9).Bold();
                        });

                        // 📌 Teléfono 1 (ancho fijo)
                        row.ConstantItem(80).Text(txt =>
                        {
                            txt.Span("Tel: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                            txt.Span(a.Telefono1).FontSize(9).Bold();
                        });

                        // 📌 Teléfono 2 (solo si existe)
                        if (!string.IsNullOrWhiteSpace(a.Telefono2))
                        {
                            row.ConstantItem(80).Text(txt =>
                            {
                                txt.Span("Tel 2: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                                txt.Span(a.Telefono2).FontSize(9).Bold();
                            });
                        }

                        // 📌 Tipo cliente (ocupa lo restante)
                        row.RelativeItem(1).PaddingLeft(15).Text(txt =>
                        {
                            txt.Span("Tipo cliente: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                            txt.Span(a.TipoCliente).FontSize(9).Bold();
                        });



                    });
                });

                // --- VEHÍCULO ---
                column.Item().PaddingTop(4).Border(1).BorderColor(GrisClaro).Padding(6).Column(col =>
                {
                    col.Item().Text("VEHÍCULO").FontSize(9).Bold().FontColor(RojoOscuro).AlignCenter();

                    // Fila 1: Marca, Modelo, Versión, Año
                    col.Item().PaddingTop(3).Row(row =>
                    {
                        row.RelativeItem().Text(txt =>
                        {
                            txt.Span("Marca: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                            txt.Span(a.Marca).FontSize(9).Bold();
                        });
                        row.RelativeItem().Text(txt =>
                        {
                            txt.Span("Modelo: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                            txt.Span(a.Modelo).FontSize(9).Bold();
                        });
                        row.RelativeItem().Text(txt =>
                        {
                            txt.Span("Versión: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                            txt.Span(a.Version).FontSize(9).Bold();
                        });
                        row.RelativeItem().Text(txt =>
                        {
                            txt.Span("Año: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                            txt.Span(a.Anio.ToString()).FontSize(9).Bold();
                        });
                    });

                    // Fila 2: Color, Km, Placas, VIN
                    col.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Text(txt =>
                        {
                            txt.Span("Color: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                            txt.Span(a.Color ?? "—").FontSize(8).Bold();
                        });
                        row.RelativeItem().Text(txt =>
                        {
                            txt.Span("Kilometraje: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                            txt.Span($"{a.Kilometraje:N0} Km").FontSize(8).Bold();
                        });
                        row.RelativeItem().Text(txt =>
                        {
                            txt.Span("Placas: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                            txt.Span(string.IsNullOrWhiteSpace(a.Placas) ? "S/P" : a.Placas).FontSize(8).Bold();
                        });
                        row.RelativeItem().Text(txt =>
                        {
                            txt.Span("VIN: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                            txt.Span(a.VIN).FontSize(8).Bold();
                        });
                    });

                    // Fila 3+: Equipamiento si existe
                    if (e != null)
                    {
                        col.Item().PaddingTop(2).Row(row =>
                        {
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("Motor: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                                txt.Span(e.Motor).FontSize(8).Bold();
                            });
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("Cilindros: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                                txt.Span(e.CantidadCilindros.ToString()).FontSize(8).Bold();
                            });
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("Puertas: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                                txt.Span(e.CantidadPuertas.ToString()).FontSize(8).Bold();
                            });
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("Transmisión: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                                txt.Span(e.TransmisionAutomatica ? "Automática" :
                                         e.TransmisionManual ? "Manual" : "—").FontSize(8).Bold();
                            });
                        });

                        col.Item().PaddingTop(2).Row(row =>
                        {
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("Vestiduras: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                                txt.Span(e.Vestiduras).FontSize(8).Bold();
                            });
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("Llantas Del.: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                                txt.Span(e.MarcaLlantasDelanteras).FontSize(8).Bold();
                                if (e.VidaUtilLlantasDelanteras.HasValue)
                                    txt.Span($" ({e.VidaUtilLlantasDelanteras}%)").FontSize(8);
                            });
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("Llantas Tras.: ").FontSize(8).FontColor(Colors.Grey.Darken2);
                                txt.Span(e.MarcaLlantasTraseras).FontSize(8).Bold();
                                if (e.VidaUtilLlantasTraseras.HasValue)
                                    txt.Span($" ({e.VidaUtilLlantasTraseras}%)").FontSize(8);
                            });
                            row.RelativeItem(); 
                        });

                        if (!string.IsNullOrWhiteSpace(a.CuentaDeVehiculo) && a.CuentaDeVehiculo != "No Aplica")
                        {
                            col.Item().PaddingTop(3)
                                .Background(Colors.Orange.Lighten4).Padding(3)
                                .Text($"⚠ A Cuenta de : {a.CuentaDeVehiculo}")
                                .FontSize(7.5f).Bold().FontColor(Colors.Orange.Darken2);
                        }
                    }
                });
            });
        }

        // -------------------------------------------------------
        // SECCIÓN 2 — Documentación (4 columnas, 2 filas compactas)
        // -------------------------------------------------------

        private void SeccionDocumentacion(IContainer container)
        {
            var e = _data.Equipamiento;
            if (e == null) return;

            container.Column(column =>
            {
                // Título compacto
                column.Item().Background(RojoOscuro).Padding(3)
                    .Text("DOCUMENTACIÓN").FontSize(9).Bold().FontColor(Colors.White);

                column.Item().PaddingTop(4).Table(table =>
                {
                    // 4 columnas: label | valor | label | valor
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(1);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(1);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(1);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(1);
                    });

                    // Fila 1: Factura | Duplicado llave | Carnet servicios | Refacturaciones
                    DocCeldaTexto(table, "Núm. dueños :", e.NumeroDuenos.ToString());                    
                    DocCeldaTexto(table, "Última Verficación :", e.Verificacion.HasValue ? e.Verificacion.ToString()! : "—");
                    DocCelda(table, "Carnet de servicios :", e.CarnetServicios);
                    DocCelda(table, "Factura original :", e.FacturaOriginal);
                    


                    // Fila 2: Núm. dueños | Última tenencia | Verificación | (vacío)
                    DocCeldaTexto(table, "Refacturaciones :", e.Refacturaciones > 0 ? e.Refacturaciones.ToString() : "0");   
                    DocCeldaTexto(table, "Última tenencia :", e.UltimaTenenciaPagada.HasValue ? e.UltimaTenenciaPagada.ToString()! : "—");
                    DocCelda(table, "Duplicado de llave :", e.DuplicadoLlave);



                    // celda vacía al final de fila 2
                    table.Cell().Padding(3);
                    table.Cell().Padding(3);
                });

            });
        }

        // -------------------------------------------------------
        // SECCIÓN 3 — Equipamiento (checklist)
        // -------------------------------------------------------

        private void SeccionEquipamiento(IContainer container)
        {
            var e = _data.Equipamiento!;

            container.Column(column =>
            {
                column.Item().Background(RojoOscuro).Padding(3)
                    .Text("EQUIPAMIENTO").FontSize(9).Bold().FontColor(Colors.White);

                column.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn();
                        cols.ConstantColumn(20);
                        cols.RelativeColumn();
                        cols.ConstantColumn(20);
                        cols.RelativeColumn();
                        cols.ConstantColumn(20);
                        cols.RelativeColumn();
                        cols.ConstantColumn(20);
                        cols.RelativeColumn();
                        cols.ConstantColumn(20);
                        cols.RelativeColumn();
                        cols.ConstantColumn(20);
                    });

                    var items = new (string nombre, bool valor)[]
                    {
                        ("ACC",                e.ACC),
                        ("Quemacocos",         e.Quemacocos),
                        ("Espejos eléctricos", e.EspejosElectricos),
                        ("Seguros eléctricos", e.SegurosElectricos),
                        ("Cristales eléct.",   e.CristalesElectricos),
                        ("Asientos eléct.",    e.AsientosElectricos),
                        ("Faros niebla",       e.FarosNiebla),
                        ("Rines aluminio",     e.RinesAluminio),
                        ("Controles volante",  e.ControlesVolante),
                        ("Estéreo/CD",         e.EstereoCD),
                        ("ABS",                e.ABS),
                        ("Dirección asistida", e.DireccionAsistida),
                        ("Bolsas de aire",     e.BolsasAire),
                        ("Bluetooth",          e.Bluetooth),
                        ("Pantalla",           e.Pantalla),
                        ("GPS",                e.GPS),
                        ("Tracción 4x4",       e.Traccion4x4),
                        ("Turbo",              e.Turbo),
                    };

                    for (int i = 0; i < items.Length; i += 6)
                    {
                        for (int j = 0; j < 6; j++)
                        {
                            if (i + j < items.Length)
                            {
                                var item = items[i + j];
                                table.Cell().Padding(3)
                                    .Text(item.nombre).FontSize(7);
                                table.Cell().PaddingTop(1).PaddingRight(18)
                                    .Text(item.valor ? "✓" : "✗")
                                    .FontSize(8)
                                    .FontColor(item.valor ? Colors.Green.Darken2 : Colors.Red.Medium);
                            }
                            else
                            {
                                table.Cell();
                                table.Cell().BorderBottom(1).BorderColor(GrisClaro);
                            }
                        }
                    }
                });

                if (!string.IsNullOrWhiteSpace(e.EquipoAdicional))
                {
                    column.Item().PaddingTop(3).Padding(5).Row(row =>
                    {
                        // El título ocupa solo lo que mide el texto
                        row.ConstantItem(70).Text("Equipo adicional: ")
                            .FontSize(8).Bold().FontColor(RojoOscuro)
                            .AlignLeft();

                        // El valor se coloca justo enseguida
                        row.RelativeItem().Text(e.EquipoAdicional)
                            .FontSize(8)
                            .AlignLeft();
                    });
                }
            });
        }

        // -------------------------------------------------------
        // SECCIÓN 4 — Reparaciones necesarias
        // -------------------------------------------------------

        private void SeccionReparaciones(IContainer container)
        {
            var reparaciones = _data.Reparaciones!;
            var avaluo = _data.Avaluo!;

            container.Column(column =>
            {
                column.Item().Background(RojoOscuro).Padding(3).Text("REACONDICIONAMIENTO").FontSize(9).Bold().FontColor(Colors.White);
                column.Item().PaddingTop(0).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(4);
                        cols.ConstantColumn(90);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(GrisLighten3).Padding(4)
                            .Text("Reparación Necesaria").FontSize(8).Bold();
                        header.Cell().Background(GrisLighten3).Padding(4)
                            .Text("Descripción").FontSize(8).Bold();
                        header.Cell().Background(GrisLighten3).AlignRight().Padding(4)
                            .Text("Costo aprox.").FontSize(8).Bold();
                    });

                    foreach (var r in reparaciones)
                    {
                        table.Cell().BorderBottom(1).BorderColor(GrisClaro).Padding(4)
                            .Text(r.ReparacionNecesaria).FontSize(8);
                        table.Cell().BorderBottom(1).BorderColor(GrisClaro).Padding(4)
                            .Text(string.IsNullOrWhiteSpace(r.DescripcionReparacion) ? "—" : r.DescripcionReparacion)
                            .FontSize(8).Italic();
                        table.Cell().BorderBottom(1).BorderColor(GrisClaro).AlignRight().Padding(4)
                            .Text($"${r.CostoAproximado:N2}").FontSize(8).Bold();
                    }
                });

                column.Item().PaddingTop(4).AlignRight().Width(180).Padding(4).Row(row =>
                    {
                        row.RelativeItem().Text("COSTO TOTAL APROX:")
                            .FontSize(8).Bold().FontColor(Colors.Black);
                        row.ConstantItem(80).AlignRight()
                            .Text($"${avaluo.CostoAproximadoReacondicionamiento:N2}")
                            .FontSize(9).Bold().FontColor(Colors.Black);
                    });
            });
        }

        // -------------------------------------------------------
        // SECCIÓN 5 — Costos / Precios
        // -------------------------------------------------------

        private void SeccionCostos(IContainer container)
        {
            var a = _data.Avaluo!;

            container.Column(column =>
            {

                column.Item().PaddingTop(4).AlignRight().Width(280)
                    .Border(1).BorderColor(RojoOscuro).Padding(8).Column(col =>
                    {
                        FilaPrecio(col, "Precio solicitado:", $"${a.PrecioSolicitado:N2}");
                        FilaPrecio(col, "Costo aprox. reacondicionamiento:",
                            $"${a.CostoAproximadoReacondicionamiento:N2}");

                        col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Black);

                        col.Item().PaddingTop(4).Background(RojoOscuro).Padding(5).Row(row =>
                        {
                            row.RelativeItem().Text("PRECIO AUTORIZADO:")
                                .FontSize(11).Bold().FontColor(Colors.White);
                            row.ConstantItem(100).AlignRight()
                                .Text(a.PrecioAutorizado > 0
                                    ? $"${a.PrecioAutorizado:N2}"
                                    : "Pendiente")
                                .FontSize(11).Bold().FontColor(Colors.White);
                        });
                    });
            });
        }

        // -------------------------------------------------------
        // SECCIÓN 6 — Notas legales
        // -------------------------------------------------------
        private void SeccionNotas(IContainer container)
        {
            container.Border(1).BorderColor(GrisClaro).Padding(8).Column(col =>
            {
                col.Item().PaddingTop(4).Text(text =>
                {
                    text.Span("NOTA: ")
                        .FontSize(7)
                        .Bold()
                        .FontColor(RojoOscuro);

                    text.Span("EL IMPORTE DE ESTE PRESUPUESTO ES APROXIMADO Y PUEDE VARIAR EN HASTA UN 15% Y TIENE " +
                              "VALIDEZ DURANTE LOS PRÓXIMOS 8 DÍAS.")
                        .FontSize(7);
                });

                col.Item().PaddingTop(4).Text(text =>
                {
                    text.Span("NOTA 1: ")
                        .FontSize(7)
                        .Bold()
                        .FontColor(RojoOscuro);

                    text.Span("El precio valuado está considerando la revisión mecánica, verificación de golpes en chasis " +
                              "o carrocería, verificación del estado de la suspensión, transmisión y motor del vehículo. " +
                              "Este avalúo está sujeto a cambios por políticas de unidades nuevas y seminuevas.")
                        .FontSize(7.5f);
                });

                col.Item().PaddingTop(4).Text(text =>
                {
                    text.Span("NOTA 2: ")
                        .FontSize(7)
                        .Bold()
                        .FontColor(RojoOscuro);

                    text.Span("La aceptación de la unidad que se está avaluando está condicionada a la revisión de la " +
                              "documentación que ampara: factura original, facturas anteriores consecutivas en copia " +
                              "legible sin tachones ni raspaduras. No se aceptan con facturas de aseguradoras (robadas " +
                              "o siniestradas) ni facturas notariadas. CUALQUIER MODIFICACIÓN SE VERÁ AFECTADA EN EL " +
                              "PRECIO DEL CONVENIO.")
                        .FontSize(7.5f);
                });
            });

        }

        // -------------------------------------------------------
        // SECCIÓN 7 — Firmas
        // -------------------------------------------------------

        private void SeccionFirmas(IContainer container)
        {
            container.Row(row =>
            {
                LineaFirma(row.RelativeItem(), "Nombre y firma del vendedor");
                row.ConstantItem(20);
                LineaFirma(row.RelativeItem(), "Autorización — Dpto. de Seminuevos");
                row.ConstantItem(20);
                LineaFirma(row.RelativeItem(), "Valuador de Seminuevos");
            });
        }

        // -------------------------------------------------------
        // PIE DE PÁGINA
        // -------------------------------------------------------

        private void CrearPiePagina(IContainer container)
        {
            container.AlignBottom().Column(column =>
            {


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

        // -------------------------------------------------------
        // HELPERS
        // -------------------------------------------------------

        private static void FilaPrecio(ColumnDescriptor col, string label, string valor)
        {
            col.Item().PaddingTop(4).Row(r =>
            {
                r.RelativeItem().Text(label).FontSize(9);
                r.ConstantItem(100).AlignRight().Text(valor).FontSize(9).Bold();
            });
        }

        /// <summary>Celda de documentación con ícono ✓/✗ — para tabla de 8 columnas.</summary>
        private static void DocCelda(TableDescriptor table, string nombre, bool valor)
        {
            table.Cell().Padding(3)
                .Text(nombre).FontSize(7.5f);
            table.Cell().Padding(3)
                .Text(valor ? "Sí" : "No")
                .FontSize(7.5f).Bold()
                .FontColor(valor ? Colors.Green.Darken2 : Colors.Red.Medium);
        }

        /// <summary>Celda de documentación con texto personalizado.</summary>
        private static void DocCeldaTexto(TableDescriptor table, string nombre, string texto)
        {
            table.Cell().Padding(3)
                .Text(nombre).FontSize(7.5f);
            table.Cell().Padding(3)
                .Text(texto)
                .FontSize(7.5f).Bold()
                .FontColor(Colors.Black);
        }

        private static void LineaFirma(IContainer slot, string etiqueta)
        {
            slot.Column(col =>
            {
                col.Item().PaddingTop(30).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                col.Item().PaddingTop(4).Text(etiqueta)
                    .FontSize(8).AlignCenter().FontColor(Colors.Grey.Darken2);
            });
        }
    }
}