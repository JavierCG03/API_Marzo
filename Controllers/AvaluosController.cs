using CarSlineAPI.Data;
using CarSlineAPI.Models.DTOs;
using CarSlineAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using static QuestPDF.Helpers.Colors;


namespace CarSlineAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvaluosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AvaluosController> _logger;
        private readonly string _rutaBaseAvaluos = @"C:\Users\Carsline\Downloads\Avaluos";

        public AvaluosController(ApplicationDbContext db, ILogger<AvaluosController> logger)
        {
            _db = db;
            _logger = logger;

            if (!Directory.Exists(_rutaBaseAvaluos))
                Directory.CreateDirectory(_rutaBaseAvaluos);
        }

        // ============================================
        // POST: api/Avaluos/crear
        // ============================================
        /// <summary>
        /// Crear nuevo avalúo con datos del vehículo y cliente
        /// </summary>
        [HttpPost("crear")]
        [ProducesResponseType(typeof(CrearAvaluoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CrearAvaluo([FromBody] CrearAvaluoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new CrearAvaluoResponse
                {
                    Success = false,
                    Message = "Datos inválidos: " + string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });

            try
            {
                // Verificar que el asesor existe
                var asesor = await _db.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == request.AsesorId && u.Activo);

                if (asesor == null)
                    return BadRequest(new CrearAvaluoResponse
                    {
                        Success = false,
                        Message = "El asesor no existe o no está activo"
                    });

                var avaluo = new DatosAvaluo
                {
                    AsesorId = request.AsesorId,
                    NombreCompleto = request.NombreCompleto,
                    TipoCliente = request.TipoCliente,
                    Telefono1 = request.Telefono1,
                    Telefono2 = request.Telefono2,
                    Marca = request.Marca,
                    Modelo = request.Modelo,
                    Version = request.Version,
                    Anio = request.Anio,
                    Color = request.Color,
                    VIN = request.VIN.ToUpperInvariant(),
                    Placas = request.Placas?.ToUpperInvariant(),
                    PlacasEdo = request.PlacasEdo,
                    Kilometraje = request.Kilometraje,
                    CuentaDeVehiculo = request.CuentaDeVehiculo,
                    PrecioSolicitado = request.PrecioSolicitado,
                    FechaAvaluo = DateTime.Now,
                    VehiculoApto = true,
                    Activo = true
                };

                _db.DatosAvaluos.Add(avaluo);
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    $"Avalúo ID {avaluo.Id} creado por Asesor {request.AsesorId} - VIN: {avaluo.VIN}");

                return Ok(new CrearAvaluoResponse
                {
                    Success = true,
                    Message = "Avalúo creado exitosamente",
                    AvaluoId = avaluo.Id,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear avalúo");
                return StatusCode(500, new CrearAvaluoResponse
                {
                    Success = false,
                    Message = "Error al crear avalúo"
                });
            }
        }


        // ============================================
        // POST: api/Avaluos/Documentacion
        // ============================================
        /// <summary>
        /// Registrar Documentacion del vehículo avaluado
        /// </summary>
        [HttpPost("Documentacion")]
        [ProducesResponseType(typeof(CrearDocumentacionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RegistrarDocumentacion([FromBody] CrearDocumentacionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new CrearDocumentacionResponse
                {
                    Success = false,
                    Message = "Datos inválidos"
                });

            try
            {
                // Verificar que el avalúo existe
                var avaluo = await _db.DatosAvaluos
                    .FirstOrDefaultAsync(a => a.Id == request.AvaluoId && a.Activo);

                if (avaluo == null)
                    return NotFound(new CrearDocumentacionResponse
                    {
                        Success = false,
                        Message = "Avalúo no encontrado"
                    });
                DocumentosAvaluo nuevaDocumentacion = null;
                var documentacionExistente = await _db.DocumentosAvaluos
                    .FirstOrDefaultAsync(d => d.AvaluoId == request.AvaluoId);

                if (documentacionExistente != null)
                {

                    documentacionExistente.AsesorId = request.AsesorId;
                    documentacionExistente.CarnetServicios = request.CarnetServicios;
                    documentacionExistente.UltimoServicioRegistrado = request.UltimoServicioRegistrado;
                    documentacionExistente.UltimaTenenciaPagada = request.UltimaTenenciaPagada;
                    documentacionExistente.UltimaVerificacionPagada = request.UltimaVerificacionPagada;
                    documentacionExistente.FacturaOriginal = request.FacturaOriginal;
                    documentacionExistente.NumeroDuenos = request.NumeroDuenos;
                    documentacionExistente.Refacturaciones = request.Refacturaciones;
                    documentacionExistente.DocumentacionCompleta = request.DocumentacionCompleta;
                    documentacionExistente.ComentariosAvaluoDocumentos = request.ComentariosAvaluoDocumentos;

                    _db.DocumentosAvaluos.Update(documentacionExistente);
                }
                else
                {
                    nuevaDocumentacion = new DocumentosAvaluo
                    {
                        AvaluoId = request.AvaluoId,
                        AsesorId = request.AsesorId,
                        CarnetServicios = request.CarnetServicios,
                        UltimoServicioRegistrado = request.UltimoServicioRegistrado,
                        UltimaTenenciaPagada = request.UltimaTenenciaPagada,
                        UltimaVerificacionPagada = request.UltimaVerificacionPagada,
                        FacturaOriginal = request.FacturaOriginal,
                        NumeroDuenos = request.NumeroDuenos,
                        Refacturaciones = request.Refacturaciones,
                        DocumentacionCompleta = request.DocumentacionCompleta,
                        ComentariosAvaluoDocumentos = request.ComentariosAvaluoDocumentos,
                    };

                    _db.DocumentosAvaluos.Add(nuevaDocumentacion);
                }

                // Siempre marcar como que ya tiene documentación
                avaluo.AvaluoDocumentos = true;

                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    $"Equipamiento registrado para Avalúo ID {request.AvaluoId}");

                return Ok(new CrearDocumentacionResponse
                {
                    Success = true,
                    Message = documentacionExistente != null
                        ? "Documentación actualizada correctamente"
                        : "Documentación registrada exitosamente",
                    DocumentacionId = documentacionExistente?.Id ?? nuevaDocumentacion.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al registrar documentacion del avalúo {request.AvaluoId}");
                return StatusCode(500, new CrearDocumentacionResponse
                {
                    Success = false,
                    Message = "Error al registrar documentacion de equipamiento"
                });
            }
        }


        // ============================================
        // POST: api/Avaluos/AvaluoMecanico
        // ============================================
        /// <summary>
        /// Registrar componentes y estado mecanico del vehiculo asi como las reparaciones necesarias para el reacondicionamiento 
        /// </summary>
        [HttpPost("AvaluoMecanico")]
        [ProducesResponseType(typeof(CrearAvaluoMecanicoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CrearAvaluoMecacnico([FromBody] CrearAvaluoMecanicoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new CrearAvaluoMecanicoResponse
                {
                    Success = false,
                    Message = "Datos inválidos"
                });
            try
            {

                var avaluo = await _db.DatosAvaluos.FirstOrDefaultAsync(a => a.Id == request.AvaluoId && a.Activo);

                if (avaluo == null)
                    return NotFound(new CrearAvaluoMecanicoResponse
                    {
                        Success = false,
                        Message = "Avalúo no encontrado"
                    });

                var existeAvaluoMecanico = avaluo.AvaluoMecanico;

                if (existeAvaluoMecanico)
                    return BadRequest(new CrearAvaluoMecanicoResponse
                    {
                        Success = false,
                        Message = "Ya existe un Avaluo Mecanico registrado para este Vehiculo"
                    });

                var AvaluoMecanico = new AvaluoMecanico
                {
                    AvaluoId = request.AvaluoId,
                    TecnicoId = request.TecnicoId,
                    Combustible = request.Combustible,
                    Motor = request.Motor,
                    Turbo = request.Turbo,
                    CantidadCilindros = request.CantidadCilindros,
                    Transmision = request.Transmision,
                    MarcaLlantasDelanteras =request.MarcaLlantasDelanteras,
                    VidaUtilLlantasDelanteras =request.VidaUtilLlantasDelanteras,
                    MarcaLlantasTraseras = request.MarcaLlantasTraseras,
                    VidaUtilLlantasTraseras = request.VidaUtilLlantasTraseras,
                    ComentariosAvaluoMecanico = request.ComentariosAvaluoMecanico,
                };
                _db.AvaluosMecanicos.Add(AvaluoMecanico);

                var reparacionesGuardadas = new List<ReparacionAvaluo>();
                foreach (var item in request.Reparaciones)
                {
                    var reparacion = new ReparacionAvaluo
                    {
                        AvaluoId = request.AvaluoId,
                        ReparacionNecesaria = item.Reparacion,
                        DescripcionReparacion = item.DescripcionReparacion,
                        CostoAproximado = item.CostoAproximado
                    };

                    _db.ReparacionesAvaluos.Add(reparacion);
                    reparacionesGuardadas.Add(reparacion);
                }

                avaluo.AvaluoMecanico = true;
                avaluo.TecnicoId = request.TecnicoId;

                await _db.SaveChangesAsync();

                _logger.LogInformation($"Avaluo Mecanico registrado para Avalúo ID {request.AvaluoId}");

                return Ok(new CrearAvaluoMecanicoResponse
                {
                    Success = true,
                    Message = "Avaluo Mecanico registrado exitosamente",
                    AvaluoMecanicoId = AvaluoMecanico.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al registrar el avalúo Mecanico {request.AvaluoId}");
                return StatusCode(500, new CrearAvaluoMecanicoResponse
                {
                    Success = false,
                    Message = "Error al registrar equipamiento"
                });
            }
        }


        // ============================================
        // POST: api/Avaluos/equipamiento
        // ============================================
        /// <summary>
        /// Registrar equipamiento del vehículo avaluado
        /// </summary>
        [HttpPost("equipamiento")]
        [ProducesResponseType(typeof(CrearEquipamientoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CrearEquipamiento([FromBody] CrearEquipamientoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new CrearEquipamientoResponse
                {
                    Success = false,
                    Message = "Datos inválidos"
                });

            try
            {
                // Verificar que el avalúo existe
                var avaluo = await _db.DatosAvaluos
                    .FirstOrDefaultAsync(a => a.Id == request.AvaluoId && a.Activo);

                if (avaluo == null)
                    return NotFound(new CrearEquipamientoResponse
                    {
                        Success = false,
                        Message = "Avalúo no encontrado"
                    });

                // Verificar que no exista ya equipamiento para este avalúo
                var existeEquipamiento = avaluo.AvaluoEquipamiento;

                if (existeEquipamiento)
                    return BadRequest(new CrearEquipamientoResponse
                    {
                        Success = false,
                        Message = "Ya existe equipamiento registrado para este avalúo"
                    });

                var equipamiento = new EquipamientoAvaluo
                {
                    AvaluoId = request.AvaluoId,
                    AsesorId = request.AsesorId,
                    Herramienta = request.Herramienta,
                    LLantaRefaccion = request.LLantaRefaccion,
                    BirloSeguridad = request.BirloSeguridad,
                    Manuales = request.Manuales,
                    DuplicadoLlave = request.DuplicadoLlave,
                    ACC = request.ACC,
                    Quemacocos = request.Quemacocos,
                    EspejosElectricos = request.EspejosElectricos,
                    SegurosElectricos = request.SegurosElectricos,
                    CristalesElectricos = request.CristalesElectricos,
                    AsientosElectricos = request.AsientosElectricos,
                    FarosNiebla = request.FarosNiebla,
                    RinesAluminio = request.RinesAluminio,
                    ControlesVolante = request.ControlesVolante,
                    EstereoCD = request.EstereoCD,
                    ABS = request.ABS,
                    DireccionAsistida = request.DireccionAsistida,
                    BolsasAire = request.BolsasAire,
                    Traccion4x4 = request.Traccion4x4,
                    Bluetooth = request.Bluetooth,
                    USB = request.USB,
                    Pantalla = request.Pantalla,
                    GPS = request.GPS,
                    CantidadPuertas = request.CantidadPuertas,
                    CantidadPasajeros = request.CantidadPasajeros,
                    Vestiduras = request.Vestiduras,
                    EquipoAdicional =request.EquipoAdicional,
                    ComentariosEquimapiento = request.ComentariosEquimapiento

                };

                avaluo.AvaluoEquipamiento = true;

                _db.EquipamientoAvaluos.Add(equipamiento);
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    $"Equipamiento registrado para Avalúo ID {request.AvaluoId}");

                return Ok(new CrearEquipamientoResponse
                {
                    Success = true,
                    Message = "Equipamiento registrado exitosamente",
                    EquipamientoId = equipamiento.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al registrar equipamiento del avalúo {request.AvaluoId}");
                return StatusCode(500, new CrearEquipamientoResponse
                {
                    Success = false,
                    Message = "Error al registrar equipamiento"
                });
            }
        }


        // ============================================
        // POST: api/Avaluos/reparaciones
        // ============================================
        /// <summary>
        /// Agregar reparaciones estimadas al avalúo (acepta lista)
        /// </summary>
        [HttpPost("reparaciones")]
        [ProducesResponseType(typeof(AgregarReparacionesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AgregarReparaciones([FromBody] CrearReparacionesRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new AgregarReparacionesResponse
                {
                    Success = false,
                    Message = "Datos inválidos"
                });
            try
            {
                // Verificar que el avalúo existe
                var avaluo = await _db.DatosAvaluos
                    .FirstOrDefaultAsync(a => a.Id == request.AvaluoId && a.Activo);

                if (avaluo == null)
                    return NotFound(new AgregarReparacionesResponse
                    {
                        Success = false,
                        Message = "Avalúo no encontrado"
                    });

                var reparacionesGuardadas = new List<ReparacionAvaluo>();

                foreach (var item in request.Reparaciones)
                {
                    var reparacion = new ReparacionAvaluo
                    {
                        AvaluoId = request.AvaluoId,
                        ReparacionNecesaria = item.Reparacion,
                        DescripcionReparacion = item.DescripcionReparacion,
                        CostoAproximado = item.CostoAproximado
                    };

                    _db.ReparacionesAvaluos.Add(reparacion);
                    reparacionesGuardadas.Add(reparacion);
                }

                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    $"Se agregaron {reparacionesGuardadas.Count} reparaciones al Avalúo {request.AvaluoId}. " +
                    $"Total reacondicionamiento: ${avaluo.CostoAproximadoReacondicionamiento:N2}");

                return Ok(new AgregarReparacionesResponse
                {
                    Success = true,
                    Message = $"Se agregaron {reparacionesGuardadas.Count} reparación(es) exitosamente",
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al agregar reparaciones al avalúo {request.AvaluoId}");
                return StatusCode(500, new AgregarReparacionesResponse
                {
                    Success = false,
                    Message = "Error al agregar reparaciones"
                });
            }
        }


        // ============================================
        // POST: api/Avaluos/fotos
        // ============================================
        /// <summary>
        /// Subir fotos del vehículo avaluado
        /// </summary>
        [HttpPost("fotos")]
        [ProducesResponseType(typeof(FotosAvaluoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubirFotos([FromForm] SubirFotosAvaluoModel model)
        {
            try
            {
                if (model.Imagenes == null || !model.Imagenes.Any())
                    return BadRequest(new FotosAvaluoResponse
                    {
                        Success = false,
                        Message = "No se recibieron imágenes"
                    });

                if (model.TiposFoto == null || model.TiposFoto.Count != model.Imagenes.Count)
                    return BadRequest(new FotosAvaluoResponse
                    {
                        Success = false,
                        Message = "El número de tipos de foto debe coincidir con el número de imágenes"
                    });

                // Verificar que el avalúo existe
                var avaluo = await _db.DatosAvaluos
                    .FirstOrDefaultAsync(a => a.Id == model.AvaluoId && a.Activo);

                if (avaluo == null)
                    return NotFound(new FotosAvaluoResponse
                    {
                        Success = false,
                        Message = "Avalúo no encontrado"
                    });

                // Crear carpeta para este avalúo
                string carpetaAvaluo = Path.Combine(
                    _rutaBaseAvaluos,
                    $"Avaluo_{model.AvaluoId}_{avaluo.VIN}"
                );

                if (!Directory.Exists(carpetaAvaluo))
                    Directory.CreateDirectory(carpetaAvaluo);

                var fotosGuardadas = new List<AvaluoFoto>();

                for (int i = 0; i < model.Imagenes.Count; i++)
                {
                    var imagen = model.Imagenes[i];
                    var tipoFoto = model.TiposFoto[i];

                    if (imagen.Length == 0) continue;

                    string tipoLimpio = LimpiarNombreArchivo(tipoFoto);
                    string extension = Path.GetExtension(imagen.FileName);
                    string nombreArchivo = $"{tipoLimpio}_{DateTime.Now:dd_HH_mm_ss}{extension}";
                    string rutaCompleta = Path.Combine(carpetaAvaluo, nombreArchivo);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                        await imagen.CopyToAsync(stream);

                    var foto = new AvaluoFoto
                    {
                        AvaluoId = model.AvaluoId,
                        TipoFoto = tipoFoto,
                        RutaFoto = rutaCompleta,
                        Fecha = DateTime.Now
                    };

                    _db.AvaluoFotos.Add(foto);
                    fotosGuardadas.Add(foto);
                }
                avaluo.FotografiasAvaluo = true;

                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    $"Se guardaron {fotosGuardadas.Count} fotos para Avalúo {model.AvaluoId}");

                return Ok(new FotosAvaluoResponse
                {
                    Success = true,
                    Message = $"Se guardaron {fotosGuardadas.Count} foto(s) exitosamente",
                    CantidadFotos = fotosGuardadas.Count,
                    Fotos = fotosGuardadas.Select(f => new FotoAvaluoDto
                    {
                        Id = f.Id,
                        AvaluoId = f.AvaluoId,
                        TipoFoto = f.TipoFoto,
                        RutaFoto = f.RutaFoto,
                        Fecha = f.Fecha
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al subir fotos del avalúo {model.AvaluoId}");
                return StatusCode(500, new FotosAvaluoResponse
                {
                    Success = false,
                    Message = "Error al subir fotos"
                });
            }
        }

        /// <summary>
        /// Obtener Avaluos 
        /// GET api/Avaluos/MisAvaluos/{UsuarioId}
        /// </summary>
        /// 
        [HttpGet("MisAvaluos/{UsuarioId}")]
        [ProducesResponseType(typeof(MisAvaluosResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<IActionResult> ObtenerMisAvaluos(int UsuarioId)
        {
            try
            {
                var avaluos = await _db.DatosAvaluos
                .Where(o => o.AsesorId == UsuarioId
                         && o.Activo)
                .OrderByDescending(a => a.FechaAvaluo)
                .Select(o => new AvaluoSimpleDto
                {
                    Id = o.Id,
                    Vendedor = o.NombreCompleto,
                    VehiculoCompleto = $"{o.Marca} {o.Modelo} {o.Version} / {o.Anio}",
                    VIN = o.VIN,
                    EquipamientoAvaluo = o.AvaluoEquipamiento,
                    FotosAvaluo = o.FotografiasAvaluo,
                    AvaluoDocumentos =o.AvaluoDocumentos,
                    AvaluoMecanico = o.AvaluoMecanico,
                    PrecioSolicitado = o.PrecioSolicitado,
                    PrecioAutorizado = o.PrecioAutorizado

                })
                .ToListAsync();

                MisAvaluosResponse response = new MisAvaluosResponse
                {
                    Success = true,
                    Message = avaluos.Count > 0
                        ? "Avalúos obtenidos exitosamente"
                        : "No tienes avalúos registrados",
                    Avaluos = avaluos
                };
                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener àvaluos");
                return StatusCode(500, new { Message = "Error al obtener Avaluos" });
            }
        }
        [HttpGet("AvaluosSinPrecio")]
        [ProducesResponseType(typeof(MisAvaluosResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AvaluosSinPrecio()
        {
            try
            {
                var avaluos = await _db.DatosAvaluos
                .Where(o => o.Activo && o.PrecioAutorizado == 0)
                .OrderBy(a => a.FechaAvaluo)
                .Select(o => new AvaluoSimpleDto
                {
                    Id = o.Id,
                    Vendedor = o.NombreCompleto,
                    VehiculoCompleto = $"{o.Marca} {o.Modelo} {o.Version} / {o.Anio}",
                    VIN = o.VIN,
                    EquipamientoAvaluo = o.AvaluoEquipamiento,
                    FotosAvaluo = o.FotografiasAvaluo,
                    AvaluoDocumentos = o.AvaluoDocumentos,
                    AvaluoMecanico = o.AvaluoMecanico,
                    PrecioSolicitado = o.PrecioSolicitado,
                    PrecioAutorizado = o.PrecioAutorizado

                })
                .ToListAsync();

                MisAvaluosResponse response = new MisAvaluosResponse
                {
                    Success = true,
                    Message = avaluos.Count > 0
                        ? "Avalúos obtenidos exitosamente"
                        : "No tienes avalúos registrados",
                    Avaluos = avaluos
                };
                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener àvaluos");
                return StatusCode(500, new { Message = "Error al obtener Avaluos" });
            }
        }

        [HttpGet("AvaluosPendientes")]
        [ProducesResponseType(typeof(MisAvaluosResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AvaluosPendientes()
        {
            try
            {
                var avaluos = await _db.DatosAvaluos
                .Where(o => o.Activo)
                .OrderBy(a => a.FechaAvaluo)
                .Select(o => new AvaluoSimpleDto
                {
                    Id = o.Id,
                    Vendedor = o.NombreCompleto,
                    VehiculoCompleto = $"{o.Marca} {o.Modelo} {o.Version} / {o.Anio}",
                    VIN = o.VIN,
                    EquipamientoAvaluo = o.AvaluoEquipamiento,
                    FotosAvaluo = o.FotografiasAvaluo,
                    AvaluoDocumentos = o.AvaluoDocumentos,
                    AvaluoMecanico = o.AvaluoMecanico,
                    PrecioSolicitado = o.PrecioSolicitado,
                    PrecioAutorizado = o.PrecioAutorizado

                })
                .ToListAsync();

                MisAvaluosResponse response = new MisAvaluosResponse
                {
                    Success = true,
                    Message = avaluos.Count > 0
                        ? "Avalúos obtenidos exitosamente"
                        : "No tienes avalúos registrados",
                    Avaluos = avaluos
                };
                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener àvaluos");
                return StatusCode(500, new { Message = "Error al obtener Avaluos" });
            }
        }

        [HttpGet("AvaluosInvestigacion")]
        [ProducesResponseType(typeof(MisAvaluosResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AvaluosInvestigacion()
        {
            try
            {
                var avaluos = await _db.DatosAvaluos
                .Where(o => o.Activo 
                 && o.VehiculoTomadoRevision == true)
                .OrderBy(a => a.FechaAvaluo)
                .Select(o => new AvaluoSimpleDto
                {
                    Id = o.Id,
                    Vendedor = o.NombreCompleto,
                    VehiculoCompleto = $"{o.Marca} {o.Modelo} {o.Version} / {o.Anio}",
                    VIN = o.VIN,
                    EquipamientoAvaluo = o.AvaluoEquipamiento,
                    FotosAvaluo = o.FotografiasAvaluo,
                    AvaluoDocumentos = o.AvaluoDocumentos,
                    AvaluoMecanico = o.AvaluoMecanico,
                    PrecioSolicitado = o.PrecioSolicitado,
                    PrecioAutorizado = o.PrecioAutorizado

                })
                .ToListAsync();

                MisAvaluosResponse response = new MisAvaluosResponse
                {
                    Success = true,
                    Message = avaluos.Count > 0
                        ? "Avalúos obtenidos exitosamente"
                        : "No tienes avalúos registrados",
                    Avaluos = avaluos
                };
                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener àvaluos");
                return StatusCode(500, new { Message = "Error al obtener Avaluos" });
            }
        }



        [HttpGet("DatosSimpelesAvaluo/{id}")]
        [ProducesResponseType(typeof(AvaluoDatosSimplesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerDatosSimplesAvaluo(int id)
        {
            try
            {
                var avaluo = await _db.DatosAvaluos
                    .Where(a => a.Id == id)
                    .FirstOrDefaultAsync();

                if (avaluo == null)
                    return NotFound(new AvaluoDatosSimplesResponse
                    {
                        Success = false,
                        Message = "Avalúo no encontrado"
                    });

                return Ok(new AvaluoDatosSimplesResponse
                {
                    Success = true,
                    Message = "Avalúo encontrado",
                    VehiculoCompleto = $"{avaluo.Marca} {avaluo.Modelo} {avaluo.Version} / {avaluo.Anio}",
                    VIN = avaluo.VIN,
                    Vendedor = avaluo.NombreCompleto,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener avalúo {id}");
                return StatusCode(500, new AvaluoDatosSimplesResponse
                {
                    Success = false,
                    Message = "Error al obtener avalúo"
                });
            }
        }


        // ============================================
        // GET: api/Avaluos/{id}
        // ============================================
        /// <summary>
        /// Obtener avalúo completo por ID
        /// </summary>

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AvaluoCompletoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerAvaluo(int id)
        {
            try
            {
                var baseAvaluo = await _db.DatosAvaluos
                    .Where(a => a.Id == id )
                    .Select(a => new { a.Id, a.AvaluoEquipamiento, a.AvaluoMecanico, a.AvaluoDocumentos})

                    .FirstOrDefaultAsync();
                if (baseAvaluo == null)

                    return NotFound(new AvaluoCompletoResponse
                    {
                        Success = false,
                        Message = "Avalúo no encontrado"
                    });

                var query = _db.DatosAvaluos.Where(a => a.Id == id);

                if (baseAvaluo.AvaluoEquipamiento)
                    query = query.Include(a => a.Equipamiento);

                if (baseAvaluo.AvaluoMecanico)
                {
                    query = query.Include(a => a.Reparaciones);
                    query = query.Include(a => a.MecanicoAvaluo);

                }
                if (baseAvaluo.AvaluoDocumentos)
                {
                    query = query.Include(a => a.Documentos);
                }

                query = query.Include(a => a.Tecnico);
                query = query.Include(a => a.Asesor);

                var avaluo = await query.FirstOrDefaultAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Avalúo encontrado",
                    AvaluoId = avaluo.Id,
                    Avaluo = MapearAvaluoDto(avaluo, avaluo.Asesor?.NombreCompleto ?? "", avaluo.Tecnico?.NombreCompleto ?? ""),
                    
                    Equipamiento = avaluo.AvaluoEquipamiento == true && avaluo.Equipamiento != null
                        ? MapearEquipamientoDto(avaluo.Equipamiento) : null,
                    Reparaciones = avaluo.AvaluoMecanico == true && avaluo.Reparaciones != null
                        ? avaluo.Reparaciones.Select(r => new ReparacionDto
                        {
                            Id = r.Id,
                            ReparacionNecesaria = r.ReparacionNecesaria,
                            DescripcionReparacion = string.IsNullOrWhiteSpace(r.DescripcionReparacion)
                            ? null
                            : r.DescripcionReparacion,
                            CostoAproximado = r.CostoAproximado
                        }).ToList()
                        :null,
                    AvaluoMecanico =avaluo.AvaluoMecanico == true && avaluo.MecanicoAvaluo != null
                        ? MapearAvaluoMecanico(avaluo.MecanicoAvaluo) : null,
                    Documentacion = avaluo.AvaluoDocumentos ==true  && avaluo.Documentos !=null
                        ? MapearDocumentosAvaluo (avaluo.Documentos) : null,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener avalúo {id}");
                return StatusCode(500, new AvaluoCompletoResponse
                {
                    Success = false,
                    Message = "Error al obtener avalúo"
                });
            }
        }

        // ============================================
        // PUT: api/Avaluos/TratarPrecio/{id}
        // ============================================
        /// <summary>
        /// Tratar precio del avalúo no podra superar el valor autorizado
        /// </summary>
        [HttpPut("TratarPrecio/{id}")]
        [ProducesResponseType(typeof(CrearAvaluoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> TratarPrecioAvaluo(int id, [FromBody] AutorizarAvaluoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new CrearAvaluoResponse { Success = false, Message = "Datos inválidos" });

            try
            {
                var avaluo = await _db.DatosAvaluos
                    .Include(a => a.Asesor)
                    .FirstOrDefaultAsync(a => a.Id == id && a.Activo);

                if (avaluo == null)
                    return NotFound(new CrearAvaluoResponse
                    {
                        Success = false,
                        Message = "Avalúo no encontrado"
                    });

                if (avaluo.PrecioAutorizado == 0)
                {
                    avaluo.PrecioTratado = request.PrecioAutorizado;
                }
                else
                {
                    if (request.PrecioAutorizado > avaluo.PrecioAutorizado)
                    return BadRequest(new CrearAvaluoResponse
                    {
                        Success = false,
                        Message = "El precio Tratado no puede ser mayor al precio autorizado"
                    });
                }
                avaluo.PrecioTratado = request.PrecioAutorizado;

                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    $"Avalúo {id} autorizado - Precio: ${request.PrecioAutorizado:N2}");

                return Ok(new CrearAvaluoResponse
                {
                    Success = true,
                    Message = "Avalúo autorizado exitosamente",
                    AvaluoId = avaluo.Id,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al autorizar avalúo {id}");
                return StatusCode(500, new CrearAvaluoResponse
                {
                    Success = false,
                    Message = "Error al autorizar avalúo"
                });
            }
        }


        // ============================================
        // PUT: api/Avaluos/autorizar/{id}
        // ============================================
        /// <summary>
        /// Autorizar precio del avalúo
        /// </summary>
        [HttpPut("autorizar/{id}")]
        [ProducesResponseType(typeof(CrearAvaluoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AutorizarAvaluo(int id, [FromBody] AutorizarAvaluoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new CrearAvaluoResponse { Success = false, Message = "Datos inválidos" });

            try
            {
                var avaluo = await _db.DatosAvaluos
                    .FirstOrDefaultAsync(a => a.Id == id && a.Activo);

                if (avaluo == null)
                    return NotFound(new CrearAvaluoResponse
                    {
                        Success = false,
                        Message = "Avalúo no encontrado"
                    });

                if(avaluo.PrecioTratado >request.PrecioAutorizado)
                {
                    avaluo.PrecioTratado = request.PrecioAutorizado;
                }

                avaluo.PrecioAutorizado = request.PrecioAutorizado;

                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    $"Avalúo {id} autorizado - Precio: ${request.PrecioAutorizado:N2}");

                return Ok(new CrearAvaluoResponse
                {
                    Success = true,
                    Message = "Avalúo autorizado exitosamente",
                    AvaluoId = avaluo.Id,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al autorizar avalúo {id}");
                return StatusCode(500, new CrearAvaluoResponse
                {
                    Success = false,
                    Message = "Error al autorizar avalúo"
                });
            }
        }

        // ============================================
        // PUT: api/Avaluos/CancelarAvaluo/{id}
        // ============================================
        /// <summary>
        /// Autorizar precio del avalúo
        /// </summary>
        [HttpPut("CancelarAvaluo/{id}")]
        [ProducesResponseType(typeof(CrearAvaluoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelarAvaluo(int id, [FromBody] CancelarAvaluoRequest request)
        {
            try
            {
                var avaluo = await _db.DatosAvaluos
                    .FirstOrDefaultAsync(a => a.Id == id && a.Activo);

                if (avaluo == null)
                    return NotFound(new CrearAvaluoResponse
                    {
                        Success = false,
                        Message = "Avalúo no encontrado"
                    });
                avaluo.Activo = false;
                avaluo.VehiculoApto = request.VehiculoApto;
                avaluo.ComentariosCancelacion = request.MotivoCancelacion;

                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    $"Avalúo {avaluo.Marca} {avaluo.Modelo} Cancelado");

                return Ok(new CrearAvaluoResponse
                {
                    Success = true,
                    Message = "Avalúo cancelado exitosamente",
                    AvaluoId = avaluo.Id,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al Cancelar avalúo {id}");
                return StatusCode(500, new CrearAvaluoResponse
                {
                    Success = false,
                    Message = "Error al cancelar avalúo"
                });
            }
        }

        // ============================================
        // PUT: api/Avaluos/TomarVehiculoRevision/{id}
        // ============================================
        /// <summary>
        /// Tomar vehiculo para revision
        /// </summary>
        [HttpPut("TomarVehiculoRevision/{id}")]
        [ProducesResponseType(typeof(CrearAvaluoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RevisionAvaluo(int id)
        {
            try
            {
                var avaluo = await _db.DatosAvaluos
                    .FirstOrDefaultAsync(a => a.Id == id && a.Activo);

                if (avaluo == null)
                    return NotFound(new CrearAvaluoResponse
                    {
                        Success = false,
                        Message = "Avalúo no encontrado"
                    });

                avaluo.VehiculoTomadoRevision = true;

                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    $"Avalúo {avaluo.Marca} {avaluo.Modelo} Cancelado");

                return Ok(new CrearAvaluoResponse
                {
                    Success = true,
                    Message = "Avalúo tomado a revision",
                    AvaluoId = avaluo.Id,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar estado de  avalúo {id}");
                return StatusCode(500, new CrearAvaluoResponse
                {
                    Success = false,
                    Message = "Error al actualizar  avalúo"
                });
            }
        }

        // ============================================
        // PUT: api/Avaluos/TomarVehiculoRevision/{id}
        // ============================================
        /// <summary>
        /// VehiculoComprado
        /// </summary>
        [HttpPut("VehiculoComprado/{id}")]
        public async Task<IActionResult> TomaVehiculo(int id)
        {
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _db.Database.BeginTransactionAsync();

                try
                {
                    var avaluo = await _db.DatosAvaluos
                        .FirstOrDefaultAsync(a => a.Id == id && a.Activo);

                    if (avaluo == null)
                    {
                        await transaction.RollbackAsync();

                        return NotFound(new CrearAvaluoResponse
                        {
                            Success = false,
                            Message = "Avalúo no encontrado"
                        });
                    }

                    int vehiculoId;

                    var vehiculoExistente = await _db.Vehiculos
                        .FirstOrDefaultAsync(v => v.VIN == avaluo.VIN);

                    if (vehiculoExistente != null)
                    {
                        vehiculoId = vehiculoExistente.Id;
                    }
                    else
                    {
                        var veh = new Vehiculo
                        {
                            ClienteId = 1,
                            VIN = avaluo.VIN,
                            Marca = avaluo.Marca,
                            Modelo = avaluo.Modelo,
                            Version = avaluo.Version,
                            Anio = avaluo.Anio,
                            Color = avaluo.Color,
                            Placas = "S/P",
                            KilometrajeInicial = avaluo.Kilometraje,
                            Activo = true
                        };

                        _db.Vehiculos.Add(veh);
                        await _db.SaveChangesAsync();

                        vehiculoId = veh.Id;
                    }

                    var reac = new ReacondicionamientoVehiculo
                    {
                        AvaluoId = avaluo.Id,
                        VehiculoId = vehiculoId,
                        FechaCompra = DateTime.Now,
                        TieneReacondicionamientoMecanico = false,
                        TieneReacondicionamientoEstetico = false,
                        TieneFotografias = false,
                        VehiculoListoVenta = false,
                    };

                    _db.ReacondicionamientosVehiculos.Add(reac);

                    avaluo.VehiculoComprado = true;
                    avaluo.Activo = false;

                    await _db.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Ok(new CrearAvaluoResponse
                    {
                        Success = true,
                        Message = "Avalúo Concluido, Toma concretada",
                        AvaluoId = avaluo.Id
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    _logger.LogError(ex, $"Error al comprar este Vehiculo {id}");

                    return StatusCode(500, new CrearAvaluoResponse
                    {
                        Success = false,
                        Message = "Error al comprar avalúo"
                    });
                }
            });
        }
        // ============================================
        // GET: api/Avaluos/foto/{id}
        // ============================================
        /// <summary>
        /// Obtener imagen de avalúo por ID
        /// </summary>
        [HttpGet("foto/{id}")]
        public async Task<IActionResult> ObtenerFoto(int id)
        {
            var foto = await _db.AvaluoFotos.FindAsync(id);

            if (foto == null || string.IsNullOrEmpty(foto.RutaFoto))
                return NotFound("Foto no encontrada");

            if (!System.IO.File.Exists(foto.RutaFoto))
                return NotFound("Archivo de imagen no encontrado");

            var imagen = System.IO.File.OpenRead(foto.RutaFoto);
            var extension = Path.GetExtension(foto.RutaFoto).ToLower();
            var mimeType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            return File(imagen, mimeType);
        }

        // ============================================
        // GET: api/Avaluos/Evidencias/{AvaluoId}
        // ============================================
        /// <summary>
        /// Obtener todas las imagenes de un avalúo 
        /// </summary>

        [HttpGet("Evidencias/{avaluoId}")]
        public async Task<ActionResult<IEnumerable<EvidenciaDto>>> GetEvidenciasPorOrden(int avaluoId)
        {
            var evidencias = await _db.Set<AvaluoFoto>()
                .Where(e => e.AvaluoId == avaluoId)
                .OrderBy(e => e.Fecha)
                .Select(e => new EvidenciaDto
                {
                    Id = e.Id,
                    OrdenGeneralId = e.AvaluoId,
                    RutaImagen = e.RutaFoto ?? string.Empty,
                    Descripcion = e.TipoFoto ?? string.Empty,
                    FechaRegistro = e.Fecha,
                    Activo = true 
                })
                .ToListAsync();

            if (!evidencias.Any())
            {
                return NotFound($"No se encontraron evidencias para el avaluo {avaluoId}");
            }

            return Ok(evidencias);
        }

        // ============================================
        // GET: api/Avaluos/Estadisticas
        // ============================================
        /// <summary>
        /// Obtener estadísticas de avalúos en un rango de fechas, incluye asesores sin avalúos
        /// </summary>
        [HttpGet("Estadisticas")]
        [ProducesResponseType(typeof(EstadisticasComprasResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ObtenerEstadisticas(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin)
        {
            if (fechaInicio > fechaFin)
                return BadRequest(new EstadisticasComprasResponse
                {
                    Success = false,
                    Message = "La fecha de inicio no puede ser mayor a la fecha de fin"
                });

            try
            {
                var fechaFinDia = fechaFin.Date.AddDays(1).AddTicks(-1);

                // Traer todos los asesores de compras activos (RolId == 8)
                var asesoresCompras = await _db.Usuarios
                    .Where(u => u.RolId == 8 && u.Activo)
                    .ToListAsync();

                // Traer avalúos en el rango de fechas
                var avaluos = await _db.DatosAvaluos
                    .Where(a => a.FechaAvaluo >= fechaInicio.Date
                             && a.FechaAvaluo <= fechaFinDia)
                    .ToListAsync();

                // Estadísticas globales
                int avaluosRealizados = avaluos.Count;
                int tomasConcretadas = avaluos.Count(a => a.VehiculoComprado);
                int avaluosCancelados = avaluos.Count(a => !a.Activo && !a.VehiculoComprado );
                int avaluosPendientes = avaluos.Count(a => a.Activo && !a.VehiculoComprado && a.VehiculoApto && !a.VehiculoTomadoRevision);
                int avaluosInvestigacion = avaluos.Count(a => a.Activo && !a.VehiculoComprado && a.VehiculoApto && a.VehiculoTomadoRevision);
                // Agrupar avalúos por asesor en memoria
                var avaluosPorAsesor = avaluos
                    .GroupBy(a => a.AsesorId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Cruzar todos los asesores con sus avalúos (Left Join en memoria)
                var compradores = asesoresCompras
                    .Select(asesor =>
                    {
                        var avaluosAsesor = avaluosPorAsesor.TryGetValue(asesor.Id, out var lista)
                            ? lista
                            : new List<DatosAvaluo>();

                        return new EstadisticasCompradorDTO
                        {
                            AsesorCompras = asesor.NombreCompleto,
                            AsesorId = asesor.Id,
                            AvaluoRealizados = avaluosAsesor.Count,
                            TomasConcretadas = avaluosAsesor.Count(a => a.VehiculoComprado),
                            AvaluosCancelados = avaluosAsesor.Count(a => !a.Activo && !a.VehiculoComprado),
                            AvaluosPendientes = avaluosAsesor.Count(a => a.Activo && !a.VehiculoComprado && a.VehiculoApto && !a.VehiculoTomadoRevision),
                            AvaluosInvestigacion = avaluosAsesor.Count(a => a.Activo && !a.VehiculoComprado && a.VehiculoApto && a.VehiculoTomadoRevision),
                        };
                    })
                    .OrderByDescending(e => e.TomasConcretadas)
                    .ThenBy(e => e.AsesorCompras)
                    .ToList();

                return Ok(new EstadisticasComprasResponse
                {
                    Success = true,
                    Message = $"Estadísticas del {fechaInicio:dd/MM/yyyy} al {fechaFin:dd/MM/yyyy}",
                    AvaluoRealizados =avaluosRealizados,
                    TomasConcretadas = tomasConcretadas,
                    AvaluosCancelados = avaluosCancelados,
                    AvaluosPendientes = avaluosPendientes,
                    AvaluosInvestigacion = avaluosInvestigacion,
                    Compradores = compradores
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de avalúos");
                return StatusCode(500, new EstadisticasComprasResponse
                {
                    Success = false,
                    Message = "Error al obtener estadísticas"
                });
            }
        }


        // ============================================
        // GET: api/Avaluos/DetallesCompras
        // ============================================
        /// <summary>
        /// Obtener detalle de avalúos filtrados por asesor, rango de fechas y tipo
        /// </summary>
        [HttpGet("DetallesCompras")]
        [ProducesResponseType(typeof(DetallesComprasResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerDetallesCompras([FromQuery] DetallesComprasRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new DetallesComprasResponse
                {
                    Success = false,
                    Message = "Datos inválidos: " + string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });

            if (request.FechaInicio > request.FechaFin)
                return BadRequest(new DetallesComprasResponse
                {
                    Success = false,
                    Message = "La fecha de inicio no puede ser mayor a la fecha de fin"
                });

            try
            {

                Usuario? asesor = null;

                if (request.AsesorId != 0)
                {
                    asesor = await _db.Usuarios
                        .FirstOrDefaultAsync(u => u.Id == request.AsesorId && u.RolId == 8 && u.Activo);

                    if (asesor == null)
                        return NotFound(new DetallesComprasResponse
                        {
                            Success = false,
                            Message = "Asesor de compras no encontrado o no activo"
                        });
                }

                var fechaFinDia = request.FechaFin.Date.AddDays(1).AddTicks(-1);

                var query = _db.DatosAvaluos
                    .Where(a =>
                        a.FechaAvaluo >= request.FechaInicio.Date &&
                        a.FechaAvaluo <= fechaFinDia
                    );
                if (request.AsesorId != 0)
                {
                    query = query.Where(a => a.AsesorId == request.AsesorId);
                }
                // Filtrar según tipo de avalúo
                query = request.TipoAvaluo.ToLower() switch
                {
                    "concretado" => query.Where(a => a.VehiculoComprado),
                    "cancelado" => query.Where(a => !a.Activo && !a.VehiculoComprado),
                    "pendiente" => query.Where(a => a.Activo && !a.VehiculoComprado && a.VehiculoApto && !a.VehiculoTomadoRevision),
                    "investigado" => query.Where(a => a.Activo && !a.VehiculoComprado && a.VehiculoApto && a.VehiculoTomadoRevision),
                    _ => query
                };

                var avaluos = await query
                    .OrderByDescending(a => a.FechaAvaluo)
                    .Select(a => new AvaluoDetalleAvaluoIdResponse
                    {
                        AvaluoId = a.Id,
                        Vendedor = a.NombreCompleto,
                        VehiculoCompleto = $"{a.Marca} {a.Modelo} {a.Version} / {a.Anio}",
                        VIN = a.VIN,
                    })
                    .ToListAsync();

                // Etiqueta legible del tipo
                var tipoLabel = request.TipoAvaluo.ToLower() switch
                {
                    "concretado" => "Tomas Concretadas",
                    "cancelado" => "Avalúos Cancelados",
                    "pendiente" => "Avalúos Pendientes",
                    "invetigado" => "Avalúos en Investigacion",
                    _ => request.TipoAvaluo
                };

                return Ok(new DetallesComprasResponse
                {
                    Success = true,
                    Message = avaluos.Count > 0
                                        ? $"Se encontraron {avaluos.Count} avalúo(s)"
                                        : "No se encontraron avalúos con los filtros indicados",
                    AsesorCompras = request.AsesorId == 0
                        ? "Todos los asesores"
                        : asesor.NombreCompleto,
                    TipodeAvaluos = tipoLabel,
                    Avaluos = avaluos!
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles de compras");
                return StatusCode(500, new DetallesComprasResponse
                {
                    Success = false,
                    Message = "Error al obtener detalles de compras"
                });
            }
        }

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
            PrecioTratado = a.PrecioTratado,
            CostoAproximadoReacondicionamiento = a.CostoAproximadoReacondicionamiento,
            FechaAvaluo = a.FechaAvaluo,
            Fotografias =a.FotografiasAvaluo,
            VehiculoApto = a.VehiculoApto,
            PrecioAutorizado = a.PrecioAutorizado,
            VehiculoTomadoRevision = a.VehiculoTomadoRevision,
            VehiculoComprado = a.VehiculoComprado,
            ComentariosCancelacion = a.ComentariosCancelacion,
            Activo =a.Activo
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

        private static string LimpiarNombreArchivo(string nombre)
        {
            if (string.IsNullOrEmpty(nombre)) return "foto";

            var limpio = nombre.Replace(" ", "_")
                               .Replace("á", "a").Replace("é", "e")
                               .Replace("í", "i").Replace("ó", "o")
                               .Replace("ú", "u").Replace("ñ", "n");

            return string.Join("_", limpio.Split(Path.GetInvalidFileNameChars()));
        }
    }

    // Modelo para recibir fotos por multipart/form-data
    public class SubirFotosAvaluoModel
    {
        public int AvaluoId { get; set; }
        public List<IFormFile> Imagenes { get; set; } = new();
        public List<string> TiposFoto { get; set; } = new();
    }
}