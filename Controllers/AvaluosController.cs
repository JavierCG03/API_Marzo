using CarSlineAPI.Data;
using CarSlineAPI.Models.DTOs;
using CarSlineAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                    Placas = request.Placas.ToUpperInvariant(),
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
                    TransmisionAutomatica = request.TransmisionAutomatica,
                    TransmisionManual = request.TransmisionManual,
                    Turbo = request.Turbo,
                    Traccion4x4 = request.Traccion4x4,
                    Bluetooth = request.Bluetooth,
                    USB = request.USB,
                    Pantalla = request.Pantalla,
                    GPS = request.GPS,
                    CantidadPuertas = request.CantidadPuertas,
                    Vestiduras = request.Vestiduras,
                    Motor = request.Motor,
                    CantidadCilindros = request.CantidadCilindros,
                    FacturaOriginal = request.FacturaOriginal,
                    NumeroDuenos = request.NumeroDuenos,
                    Refacturaciones = request.Refacturaciones,
                    UltimaTenenciaPagada = request.UltimaTenenciaPagada,
                    Verificacion = request.Verificacion,
                    DuplicadoLlave = request.DuplicadoLlave,
                    CarnetServicios = request.CarnetServicios,
                    EquipoAdicional = request.EquipoAdicional,
                    MarcaLlantasDelanteras = request.MarcaLlantasDelanteras,
                    VidaUtilLlantasDelanteras = request.VidaUtilLlantasDelanteras,
                    MarcaLlantasTraseras = request.MarcaLlantasTraseras,
                    VidaUtilLlantasTraseras = request.VidaUtilLlantasTraseras
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
                        TecnicoId = request.TecnicoId,
                        ReparacionNecesaria = item.Reparacion,
                        DescripcionReparacion = item.DescripcionReparacion,
                        CostoAproximado = item.CostoAproximado
                    };

                    _db.ReparacionesAvaluos.Add(reparacion);
                    reparacionesGuardadas.Add(reparacion);
                }
                avaluo.AvaluoReparaciones = true;

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
                    ReparacionesAvaluo = o.AvaluoReparaciones,
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
                    .Where(a => a.Id == id && a.Activo)
                    .Select(a => new { a.Id, a.AvaluoEquipamiento, a.AvaluoReparaciones })

                    .FirstOrDefaultAsync();
                if (baseAvaluo == null)

                    return NotFound(new AvaluoCompletoResponse
                    {
                        Success = false,
                        Message = "Avalúo no encontrado"
                    });

                var query = _db.DatosAvaluos.Where(a => a.Id == id && a.Activo);

                if (baseAvaluo.AvaluoEquipamiento)
                    query = query.Include(a => a.Equipamiento);

                if (baseAvaluo.AvaluoReparaciones)
                    query = query.Include(a => a.Reparaciones);

                query = query.Include(a => a.Asesor);

                var avaluo = await query.FirstOrDefaultAsync();


                return Ok(new
                {
                    Success = true,
                    Message = "Avalúo encontrado",
                    AvaluoId = avaluo.Id,
                    Avaluo = MapearAvaluoDto(avaluo, avaluo.Asesor?.NombreCompleto ?? ""),
                    Equipamiento = avaluo.AvaluoEquipamiento == true && avaluo.Equipamiento != null
                        ? MapearEquipamientoDto(avaluo.Equipamiento) : null,
                    Reparaciones = avaluo.AvaluoReparaciones == true && avaluo.Reparaciones != null
                        ? avaluo.Reparaciones.Select(r => new ReparacionDto
                        {
                            Id = r.Id,
                            ReparacionNecesaria = r.ReparacionNecesaria,
                            DescripcionReparacion = string.IsNullOrWhiteSpace(r.DescripcionReparacion)
                            ? null
                            : r.DescripcionReparacion,
                            CostoAproximado = r.CostoAproximado
                        }).ToList()
                        :null
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
                    .Include(a => a.Asesor)
                    .FirstOrDefaultAsync(a => a.Id == id && a.Activo);

                if (avaluo == null)
                    return NotFound(new CrearAvaluoResponse
                    {
                        Success = false,
                        Message = "Avalúo no encontrado"
                    });

                avaluo.PrecioAutorizado = request.PrecioAutorizado;
                avaluo.VehiculoApto = request.VehiculoApto;

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
        /*
        // ============================================
        // DELETE: api/Avaluos/reparacion/{id}
        // ============================================
        /// <summary>
        /// Eliminar reparación de un avalúo
        /// </summary>
        [HttpDelete("reparacion/{id}")]
        public async Task<IActionResult> EliminarReparacion(int id)
        {
            try
            {
                var reparacion = await _db.ReparacionesAvaluos.FindAsync(id);

                if (reparacion == null)
                    return NotFound(new { Success = false, Message = "Reparación no encontrada" });

                _db.ReparacionesAvaluos.Remove(reparacion);
                await _db.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Reparación eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar reparación {id}");
                return StatusCode(500, new { Success = false, Message = "Error al eliminar reparación" });
            }
        }*/

        // ============================================
        // MÉTODOS PRIVADOS
        // ============================================

        private static AvaluoDto MapearAvaluoDto(DatosAvaluo a, string asesorNombre) => new()
        {
            Id = a.Id,
            AsesorNombre = asesorNombre,
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
            Kilometraje = a.Kilometraje,
            CuentaDeVehiculo = a.CuentaDeVehiculo,
            PrecioSolicitado = a.PrecioSolicitado,
            CostoAproximadoReacondicionamiento = a.CostoAproximadoReacondicionamiento,
            FechaAvaluo = a.FechaAvaluo,
            BajaPlacas = a.BajaPlacas,
            Fotografias =a.FotografiasAvaluo,
            VehiculoApto = a.VehiculoApto,
            PrecioAutorizado = a.PrecioAutorizado,
            VehiculoTomadoRevision = a.VehiculoTomadoRevision,
            VehiculoComprado = a.VehiculoComprado
        };

        private static EquipamientoDto MapearEquipamientoDto(EquipamientoAvaluo e) => new()
        {
            Id = e.Id,
            AvaluoId = e.AvaluoId,
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
            TransmisionAutomatica = e.TransmisionAutomatica,
            TransmisionManual = e.TransmisionManual,
            Turbo = e.Turbo,
            Traccion4x4 = e.Traccion4x4,
            Bluetooth = e.Bluetooth,
            USB = e.USB,
            Pantalla = e.Pantalla,
            GPS = e.GPS,
            CantidadPuertas = e.CantidadPuertas,
            Vestiduras = e.Vestiduras,
            Motor = e.Motor,
            CantidadCilindros = e.CantidadCilindros,
            FacturaOriginal = e.FacturaOriginal,
            NumeroDuenos = e.NumeroDuenos,
            Refacturaciones = e.Refacturaciones,
            UltimaTenenciaPagada = e.UltimaTenenciaPagada,
            Verificacion = e.Verificacion,
            DuplicadoLlave = e.DuplicadoLlave,
            CarnetServicios = e.CarnetServicios,
            EquipoAdicional = e.EquipoAdicional,
            MarcaLlantasDelanteras = e.MarcaLlantasDelanteras,
            VidaUtilLlantasDelanteras = e.VidaUtilLlantasDelanteras,
            MarcaLlantasTraseras = e.MarcaLlantasTraseras,
            VidaUtilLlantasTraseras = e.VidaUtilLlantasTraseras
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