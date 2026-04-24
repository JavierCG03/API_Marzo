using CarSlineAPI.Data;
using CarSlineAPI.Models.DTOs;
using CarSlineAPI.Models.Entities;
using CarSlineAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarSlineAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IPdfService _pdfService;
        private readonly ILogger<PdfController> _logger;

        public PdfController(
            ApplicationDbContext db,
            IPdfService pdfService,
            ILogger<PdfController> logger)
        {
            _db = db;
            _pdfService = pdfService;
            _logger = logger;
        }
        /// <summary>
        /// Generar y descargar PDF de una orden
        /// GET api/Pdf/orden/{ordenId}/descargar
        /// </summary>
        [HttpGet("orden/{ordenId}/descargar")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DescargarPdfOrden(int ordenId)
        {
            try
            {
                _logger.LogInformation($"📥 Solicitud de descarga PDF para orden {ordenId}");

                var ordenDto = await ObtenerDatosOrdenAsync(ordenId);

                if (ordenDto == null)
                {
                    return NotFound(new { Message = "Orden no encontrada" });
                }

                var pdfBytes = await _pdfService.GenerarPdfOrdenAsync(ordenDto);

                _logger.LogInformation($"✅ PDF generado exitosamente: {pdfBytes.Length} bytes");

                return File(
                    pdfBytes,
                    "application/pdf",
                    $"Orden_{ordenDto.NumeroOrden}.pdf"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al generar PDF para orden {ordenId}");
                return StatusCode(500, new { Message = $"Error al generar PDF: {ex.Message}" });
            }
        }

        /// <summary>
        /// Generar y guardar PDF de una orden en el servidor
        /// POST api/Pdf/orden/{ordenId}/guardar
        /// </summary>
        [HttpPost("orden/{ordenId}/preview")]
        [ProducesResponseType(typeof(PdfPreviewResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GuardarPdfOrden(int ordenId)
        {
            try
            {
                var ordenDto = await ObtenerDatosOrdenAsync(ordenId);

                if (ordenDto == null)
                {
                    return NotFound(new { Message = "Orden no encontrada" });
                }

                var pdfBytes = await _pdfService.GuardarPdfOrdenAsync(
                    ordenDto,
                    ordenDto.NumeroOrden
                );


                return Ok(new PdfPreviewResponse
                {
                    Success = true,
                    PdfBase64 = Convert.ToBase64String(pdfBytes),
                    NumeroOrden = ordenDto.NumeroOrden
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error en vista previa PDF orden {ordenId}");
                return StatusCode(500, new PdfPreviewResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }


        /// <summary>
        /// Generar y descargar PDF de un avalúo
        /// GET api/Pdf/avaluo/{avaluoId}/descargar
        /// </summary>
        [HttpGet("avaluo/{avaluoId}/descargar")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DescargarPdfAvaluo(int avaluoId)
        {
            try
            {
                _logger.LogInformation($"📥 Solicitud de descarga PDF para avalúo {avaluoId}");

                var data = await ObtenerDatosAvaluoAsync(avaluoId);
                if (data == null)
                    return NotFound(new { Message = "Avalúo no encontrado" });

                var pdfBytes = await _pdfService.GenerarPdfAvaluoAsync(data);
                string folio = $"Avaluo_{avaluoId:D6}";

                return File(pdfBytes, "application/pdf", $"{folio}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al generar PDF para avalúo {avaluoId}");
                return StatusCode(500, new { Message = $"Error al generar PDF: {ex.Message}" });
            }
        }

        /// <summary>
        /// Vista previa en Base64 del PDF de un avalúo
        /// POST api/Pdf/avaluo/{avaluoId}/preview
        /// </summary>
        [HttpPost("avaluo/{avaluoId}/preview")]
        [ProducesResponseType(typeof(PdfPreviewResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PreviewPdfAvaluo(int avaluoId)
        {
            try
            {
                var data = await ObtenerDatosAvaluoAsync(avaluoId);
                if (data == null)
                    return NotFound(new { Message = "Avalúo no encontrado" });

                var pdfBytes = await _pdfService.GuardarPdfAvaluoAsync(data, $"Avaluo_{avaluoId:D6}");

                return Ok(new PdfPreviewResponse
                {
                    Success = true,
                    PdfBase64 = Convert.ToBase64String(pdfBytes),
                    NumeroOrden = $"AVL_{data.Avaluo.VIN}",
                    TamanoBytes = pdfBytes.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error en vista previa PDF avalúo {avaluoId}");
                return StatusCode(500, new PdfPreviewResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Vista previa en Base64 del PDF de un avalúo
        /// POST api/Pdf/avaluo/{avaluoId}/preview
        /// </summary>
        [HttpPost("CheckList/{avaluoId}/preview")]
        [ProducesResponseType(typeof(PdfPreviewResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PreviewPdfCheckLsit(int avaluoId)
        {
            try
            {
                var data = await ObtenerDatosCheckListAsync(avaluoId);
                if (data == null)
                    return NotFound(new { Message = "Avalúo no encontrado" });

                var pdfBytes = await _pdfService.GuardarPdfCheckListAsync(data, $"Avaluo_{avaluoId:D6}");

                return Ok(new PdfPreviewResponse
                {
                    Success = true,
                    PdfBase64 = Convert.ToBase64String(pdfBytes),
                    NumeroOrden = $"CHECKLIST_{data.Avaluo.VIN}",
                    TamanoBytes = pdfBytes.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error en vista previa PDF avalúo {avaluoId}");
                return StatusCode(500, new PdfPreviewResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }
        private async Task<OrdenPdfDto?> ObtenerDatosOrdenAsync(int ordenId)
        {
            var orden = await _db.OrdenesGenerales
                .Include(o => o.Cliente)
                .Include(o => o.Vehiculo)
                .Include(o => o.Asesor)
                .Include(o => o.TipoOrden)
                .Include(o => o.EstadoOrden)
                .Include(o => o.Trabajos.Where(t => t.Activo))
                    .ThenInclude(t => t.TecnicoAsignado)
                .Include(o => o.Trabajos)
                    .ThenInclude(t => t.EstadoTrabajoNavegacion)
                .Where(o => o.Id == ordenId && o.Activo)
                .FirstOrDefaultAsync();

            if (orden == null) return null;

            // Obtener refacciones de cada trabajo
            var trabajosConRefacciones = new List<TrabajoPdfDto>();

            foreach (var trabajo in orden.Trabajos.Where(t => t.Activo))
            {
                var refacciones = await _db.Set<Refacciontrabajo>()
                    .Where(r => r.TrabajoId == trabajo.Id)
                    .Select(r => new RefaccionPdfDto
                    {
                        Refaccion = r.Refaccion,
                        Cantidad = r.Cantidad,
                        PrecioUnitario = r.PrecioUnitario,
                        Total = r.Cantidad * r.PrecioUnitario
                    })
                    .ToListAsync();

                var duracion = trabajo.FechaHoraInicio.HasValue && trabajo.FechaHoraTermino.HasValue
                    ? (trabajo.FechaHoraTermino.Value - trabajo.FechaHoraInicio.Value)
                    : (TimeSpan?)null;

                trabajosConRefacciones.Add(new TrabajoPdfDto
                {
                    Trabajo = trabajo.Trabajo,
                    TecnicoNombre = trabajo.TecnicoAsignado?.NombreCompleto,
                    EstadoTrabajo = trabajo.EstadoTrabajoNavegacion?.NombreEstado ?? "Pendiente",
                    FechaInicio = trabajo.FechaHoraInicio,
                    FechaTermino = trabajo.FechaHoraTermino,
                    Duracion = duracion.HasValue
                        ? $"{duracion.Value.Hours}h {duracion.Value.Minutes}m"
                        : null,
                    CostoManoObra = trabajo.CostoManoObra,
                    TotalRefacciones = trabajo.RefaccionesTotal,
                    Refacciones = refacciones,
                    ComentariosTecnico = trabajo.ComentariosTecnico
                });
            }

            // Obtener checklist si existe
            var checkList = await _db.Set<CheckListServicio>()
                .Where(c => c.OrdenGeneralId == ordenId)
                .Select(c => new CheckListPdfDto
                {
                    Bieletas = c.Bieletas,
                    Terminales = c.Terminales,
                    CajaDireccion = c.CajaDireccion,
                    Volante = c.Volante,
                    AmortiguadoresDelanteros = c.AmortiguadoresDelanteros,
                    AmortiguadoresTraseros = c.AmortiguadoresTraseros,
                    BarraEstabilizadora = c.BarraEstabilizadora,
                    Horquillas = c.Horquillas,
                    NeumaticosDelanteros = c.NeumaticosDelanteros,
                    NeumaticosTraseros = c.NeumaticosTraseros,
                    Balanceo = c.Balanceo,
                    Alineacion = c.Alineacion,
                    LucesAltas = c.LucesAltas,
                    LucesBajas = c.LucesBajas,
                    LucesAntiniebla = c.LucesAntiniebla,
                    LucesReversa = c.LucesReversa,
                    LucesDireccionales = c.LucesDireccionales,
                    LucesIntermitentes = c.LucesIntermitentes,
                    DiscosTamboresDelanteros = c.DiscosTamboresDelanteros,
                    DiscosTamboresTraseros = c.DiscosTamboresTraseros,
                    BalatasDelanteras = c.BalatasDelanteras,
                    BalatasTraseras = c.BalatasTraseras,
                    ReemplazoAceiteMotor = c.ReemplazoAceiteMotor,
                    ReemplazoFiltroAceite = c.ReemplazoFiltroAceite,
                    ReemplazoFiltroAireMotor = c.ReemplazoFiltroAireMotor,
                    ReemplazoFiltroAirePolen = c.ReemplazoFiltroAirePolen,
                    DescristalizacionTamboresDiscos = c.DescristalizacionTamboresDiscos,
                    AjusteFrenos = c.AjusteFrenos,
                    CalibracionPresionNeumaticos = c.CalibracionPresionNeumaticos,
                    TorqueNeumaticos = c.TorqueNeumaticos,
                    RotacionNeumaticos = c.RotacionNeumaticos
                })
                .FirstOrDefaultAsync();

            return new OrdenPdfDto
            {
                NumeroOrden = orden.NumeroOrden,
                TipoOrden = orden.TipoOrden.NombreTipo ?? "",
                FechaCreacion = orden.FechaCreacion,
                FechaPromesaEntrega = orden.FechaHoraPromesaEntrega,
                FechaFinalizacion = orden.FechaFinalizacion,
                EstadoOrden = orden.EstadoOrden.NombreEstado ?? "",

                Cliente = new ClientePdfDto
                {
                    NombreCompleto = orden.Cliente.NombreCompleto,
                    RFC = orden.Cliente.RFC,
                    TelefonoMovil = orden.Cliente.TelefonoMovil,
                    CorreoElectronico = orden.Cliente.CorreoElectronico,
                    DireccionCompleta = $"{orden.Cliente.Calle} {orden.Cliente.NumeroExterior}, {orden.Cliente.Colonia}, {orden.Cliente.Municipio}, {orden.Cliente.Estado}"
                },

                Vehiculo = new VehiculoPdfDto
                {
                    VehiculoCompleto = $"{orden.Vehiculo.Marca} {orden.Vehiculo.Modelo} {orden.Vehiculo.Anio}",
                    VIN = orden.Vehiculo.VIN,
                    Placas = orden.Vehiculo.Placas,
                    Color = orden.Vehiculo.Color ?? "",
                    KilometrajeActual = orden.KilometrajeActual
                },

                AsesorNombre = orden.Asesor.NombreCompleto,
                Trabajos = trabajosConRefacciones,
                CheckList = checkList,

                TotalRefacciones = trabajosConRefacciones.Sum(t => t.TotalRefacciones),
                TotalManoObra = trabajosConRefacciones.Sum(t => t.CostoManoObra),
                CostoTotal = orden.CostoTotal ?? 0,
                CostoTotal_IVA = orden.CostoTotal_IVA,

                ObservacionesAsesor = orden.ObservacionesAsesor,
                ObservacionesJefeTaller = orden.ObservacionesJefe,

                TotalTrabajos = orden.TotalTrabajos,
                TrabajosCompletados = orden.TrabajosCompletados,
                ProgresoGeneral = orden.ProgresoGeneral
            };
        }


        // -------------------------------------------------------
        // Método privado que construye el AvaluoCompletoResponse
        // -------------------------------------------------------

        private async Task<AvaluoCompletoResponse?> ObtenerDatosAvaluoAsync(int avaluoId)
        {
            var avaluo = await _db.DatosAvaluos
                .Where(a => a.Id == avaluoId && a.Activo)
                .FirstOrDefaultAsync();

            if (avaluo == null) return null;

            // Equipamiento (si existe)
            EquipamientoDto? equipamientoDto = null;
            if (avaluo.AvaluoEquipamiento)
            {
                var eq = await _db.EquipamientoAvaluos.FirstOrDefaultAsync(e => e.AvaluoId == avaluoId);

                if (eq != null)
                    equipamientoDto = MapearEquipamientoDto(eq);
            }
            AvaluoDocumentosDto? avaluoDocumentos = null;
            if (avaluo.AvaluoDocumentos)
            {
                var doc = await _db.DocumentosAvaluos.FirstOrDefaultAsync(d => d.AvaluoId == avaluo.Id);
                if (doc != null)
                  avaluoDocumentos = MapearDocumentosAvaluo(doc);
            }
            AvaluoMecanicoDto? avaluoMecanico = null;
            if (avaluo.AvaluoMecanico)
            {
                var mec = await _db.AvaluosMecanicos.FirstOrDefaultAsync(m => m.AvaluoId == avaluo.Id);
                if (mec != null)
                    avaluoMecanico = MapearAvaluoMecanico(mec);
            }

            // Reparaciones (si existen)
            List<ReparacionDto>? reparacionesDto = null;
            if (avaluo.AvaluoMecanico)
            {

                reparacionesDto = await _db.ReparacionesAvaluos
                    .Where(r => r.AvaluoId == avaluoId)
                    .Select(r => new ReparacionDto
                    {
                        Id = r.Id,
                        ReparacionNecesaria = r.ReparacionNecesaria,
                        DescripcionReparacion = r.DescripcionReparacion,
                        CostoAproximado = r.CostoAproximado
                    })
                    .ToListAsync();
            }
            // Tecnico (valuador)
            string tecnicoNombre = "";
            var tecnico= await _db.Usuarios.FindAsync(avaluo.TecnicoId);
            if (tecnico!= null)
                tecnicoNombre = tecnico.NombreCompleto;

            // Asesor (valuador)
            string asesorNombre = "";
            var asesor = await _db.Usuarios.FindAsync(avaluo.AsesorId);
            if (asesor != null)
                asesorNombre = asesor.NombreCompleto;

            return new AvaluoCompletoResponse
            {
                Success = true,
                Message = "OK",
                AvaluoId = avaluo.Id,
                Avaluo = MapearAvaluoDto(avaluo, asesorNombre, tecnicoNombre),
                Equipamiento = equipamientoDto,
                Documentos = avaluoDocumentos,
                AvaluoMecanico = avaluoMecanico,
                Reparaciones = reparacionesDto ?? new List<ReparacionDto>()
                
            };
        }


        // -------------------------------------------------------
        // Método privado que construye el CheckListCompletoResponse
        // -------------------------------------------------------

        private async Task<CheckListAvaluoCompletoPdf?> ObtenerDatosCheckListAsync(int avaluoId)
        {
            var avaluo = await _db.DatosAvaluos
                .Where(a => a.Id == avaluoId && a.Activo)
                .FirstOrDefaultAsync();

            if (avaluo == null) return null;

            CheckListAvaluoDto? CheckList = null;
            { 
                var check = await _db.CheckListAvaluos.FirstOrDefaultAsync(d => d.AvaluoId == avaluo.Id);
                
                string vigilanteNombre = "";
                var vigilante = await _db.Usuarios.FindAsync(check.VigilanteId);
                if (vigilante != null)
                    vigilanteNombre = vigilante.NombreCompleto;

                if (check != null)
                    CheckList = MapearCheckListAvaluo(check, vigilanteNombre);

            }

            // Tecnico (valuador)
            string tecnicoNombre = "";
            var tecnico = await _db.Usuarios.FindAsync(avaluo.TecnicoId);
            if (tecnico != null)
                tecnicoNombre = tecnico.NombreCompleto;


            // Asesor (valuador)
            string asesorNombre = "";
            var asesor = await _db.Usuarios.FindAsync(avaluo.AsesorId);
            if (asesor != null)
                asesorNombre = asesor.NombreCompleto;

            return new CheckListAvaluoCompletoPdf
            {
                Avaluo = MapearAvaluoDto(avaluo, asesorNombre,tecnicoNombre),
                CheckList = CheckList
            };
        }

        // Reutiliza los mismos helpers de AvaluosController
        private static AvaluoDto MapearAvaluoDto(DatosAvaluo a, string asesorNombre, string tecnicoNombre) => new()
        {
            Id = a.Id,
            AsesorNombre = asesorNombre,
            TecnicoNombre = tecnicoNombre,
            NombreCompleto = a.NombreCompleto,
            TipoCliente = a.TipoCliente,
            Telefono1 = a.Telefono1,
            Telefono2 = a.Telefono2,
            Marca = a.Marca,
            Modelo = a.Modelo,
            Version = a.Version,
            Anio = a.Anio,
            Color = a.Color,
            VIN = a.VIN,
            Placas = a.Placas,
            PlacasEdo = a.PlacasEdo,
            Kilometraje = a.Kilometraje,
            CuentaDeVehiculo = a.CuentaDeVehiculo,
            PrecioSolicitado = a.PrecioSolicitado,
            PrecioTratado =a.PrecioTratado,
            CostoAproximadoReacondicionamiento = a.CostoAproximadoReacondicionamiento,
            FechaAvaluo = a.FechaAvaluo,
            Fotografias = a.FotografiasAvaluo,
            VehiculoApto = a.VehiculoApto,
            PrecioAutorizado = a.PrecioAutorizado,
            VehiculoTomadoRevision = a.VehiculoTomadoRevision,
            VehiculoComprado = a.VehiculoComprado
        };

        private static AvaluoDocumentosDto MapearDocumentosAvaluo(DocumentosAvaluo a) => new()
        {
            Id = a.Id,
            CarnetServicios = a.CarnetServicios,
            UltimoServicioRegistrado = a.UltimoServicioRegistrado,
            UltimaTenenciaPagada = a.UltimaTenenciaPagada,
            UltimaVerificacionPagada = a.UltimaVerificacionPagada,
            FacturaOriginal = a.FacturaOriginal,
            NumeroDuenos = a.NumeroDuenos,
            Refacturaciones = a.Refacturaciones,
            DocumentacionCompleta = a.DocumentacionCompleta,
            ComentariosAvaluoDocumentos = a.ComentariosAvaluoDocumentos,
        };
        private static CheckListAvaluoDto MapearCheckListAvaluo(CheckListAvaluo c, string vigilanteNombre) => new()
        {
        Id  = c.Id,
        AvaluoId =c.AvaluoId,
        VigilanteId = c.VigilanteId,
        VigilanteNombre = vigilanteNombre,
        Kilometraje = c.Kilometraje,
        Combustible = c.Combustible,

        DuplicadoLlave = c.DuplicadoLlave,
        BirloSeguridad  = c.BirloSeguridad,
        CandadoSeguridad  = c.CandadoSeguridad,
        TapaCajuela  = c.TapaCajuela,
        CortinaCajuela = c.CortinaCajuela,
        Refaccion = c.Refaccion,
        Gato = c.Gato,
        Maneral = c.Maneral,
        LlaveLoCruz = c.LlaveLoCruz,
        GanchoArrastre = c.GanchoArrastre,
        TaponGasolina = c.TaponGasolina,
        Rines = c.Rines,
        Tapones = c.Tapones,
        CentroRin = c.CentroRin,
        RadioEstereo = c.RadioEstereo,
        LimpiadorDelantero = c.LimpiadorDelantero,
        LimpiadorTrasero = c.LimpiadorTrasero,
        Viseras = c.Viseras,
        Cabeceras = c.Cabeceras,
        Tapetes = c.Tapetes,
        FarosNiebla = c.FarosNiebla,
        Bateria = c.Bateria,
        VarillasPickUp = c.VarillasPickUp, 
        Encendedor = c.Encendedor,
        AntenaRadio =c.AntenaRadio,
        Maletin =c.Maletin,
        Cubresala = c.Cubresala,
        Cubrevolante = c.Cubrevolante,
        MarcaBateria  = c.MarcaBateria,
        MarcaLlantaDelanteraDer = c.MarcaLlantaDelanteraDer,
        MarcaLlantaDelanteraIzq = c.MarcaLlantaDelanteraIzq,
        MarcaLlantaTraseraDer = c.MarcaLlantaTraseraDer,
        MarcaLlantaTraseraIzq =c.MarcaLlantaTraseraIzq,
        MedidaLlantaDelanteraDer = c.MedidaLlantaDelanteraDer,
        MedidaLlantaDelanteraIzq = c.MedidaLlantaDelanteraIzq,
        MedidaLlantaTraseraDer = c.MedidaLlantaTraseraDer,
        MedidaLlantaTraseraIzq = c.MedidaLlantaTraseraIzq,
        MarcaLlantaRefaccion = c.MedidaLlantaRefaccion,
        MedidaLlantaRefaccion = c.MedidaLlantaRefaccion,
        Comentarios = c.Comentarios,
        Observaciones = c.Observaciones,
    };
        private static AvaluoMecanicoDto MapearAvaluoMecanico(AvaluoMecanico a) => new()
        {
            Id = a.Id,
            Combustible = a.Combustible,
            Motor = a.Motor,
            Turbo = a.Turbo,
            CantidadCilindros = a.CantidadCilindros,
            Transmision = a.Transmision,
            MarcaLlantasDelanteras = a.MarcaLlantasDelanteras,
            VidaUtilLlantasDelanteras = a.VidaUtilLlantasDelanteras,
            MarcaLlantasTraseras = a.MarcaLlantasTraseras,
            VidaUtilLlantasTraseras = a.VidaUtilLlantasTraseras,
            ComentariosAvaluoMecanico = a.ComentariosAvaluoMecanico,
        };

        private static EquipamientoDto MapearEquipamientoDto(EquipamientoAvaluo e) => new()
        {
            Id = e.Id,
            AvaluoId = e.AvaluoId,
            Herramienta = e.Herramienta,
            LLantaRefaccion = e.LLantaRefaccion,
            BirloSeguridad = e.BirloSeguridad,
            Manuales = e.Manuales,
            DuplicadoLlave = e.DuplicadoLlave,
            ACC = e.ACC,
            Quemacocos = e.Quemacocos,
            EspejosElectricos = e.EspejosElectricos,
            SegurosElectricos = e.SegurosElectricos,
            CristalesElectricos = e.CristalesElectricos,
            AsientosElectricos = e.AsientosElectricos,
            FarosNiebla = e.FarosNiebla,
            RinesAluminio = e.RinesAluminio,
            ControlesVolante = e.ControlesVolante,
            EstereoCD = e.EstereoCD,
            ABS = e.ABS,
            DireccionAsistida = e.DireccionAsistida,
            BolsasAire = e.BolsasAire,
            Traccion4x4 = e.Traccion4x4,
            Bluetooth = e.Bluetooth,
            USB = e.USB,
            Pantalla = e.Pantalla,
            GPS = e.GPS,
            CantidadPuertas = e.CantidadPuertas,
            CantidadPasajeros = e.CantidadPasajeros,
            Vestiduras = e.Vestiduras,
            EquipoAdicional = e.EquipoAdicional,
            ComentariosEquipamiento = e.ComentariosEquimapiento,
        };
    }



}