// ============================================
// Controllers/ReacondicionamientoVehiculoController.cs
// ============================================
using CarSlineAPI.Data;
using CarSlineAPI.Models.DTOs;
using CarSlineAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarSlineAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReacondicionamientoVehiculoController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ReacondicionamientoVehiculoController> _logger;

        public ReacondicionamientoVehiculoController(
            ApplicationDbContext db,
            ILogger<ReacondicionamientoVehiculoController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ============================================
        // POST: api/ReacondicionamientoVehiculo/crear
        // ============================================
        /// <summary>
        /// Registrar el reacondicionamiento general de un vehículo comprado
        /// </summary>
        [HttpPost("crear")]
        [ProducesResponseType(typeof(ReacondicionamientoVehiculoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Crear([FromBody] CrearReacondicionamientoVehiculoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ReacondicionamientoVehiculoResponse
                {
                    Success = false,
                    Message = "Datos inválidos: " + string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });

            try
            {
                // Verificar que el avalúo existe y que el vehículo fue comprado
                var avaluo = await _db.DatosAvaluos
                    .FirstOrDefaultAsync(a => a.Id == request.AvaluoId && a.VehiculoComprado);

                if (avaluo == null)
                    return NotFound(new ReacondicionamientoVehiculoResponse
                    {
                        Success = false,
                        Message = "Avalúo no encontrado o el vehículo aún no ha sido comprado"
                    });

                // Verificar que no exista ya un reacondicionamiento para este avalúo
                var existente = await _db.ReacondicionamientosVehiculos
                    .AnyAsync(r => r.AvaluoId == request.AvaluoId);

                if (existente)
                    return BadRequest(new ReacondicionamientoVehiculoResponse
                    {
                        Success = false,
                        Message = "Ya existe un reacondicionamiento registrado para este avalúo"
                    });

                // Verificar que la orden mecánica existe
                var ordenMecanica = await _db.OrdenesGenerales
                    .FirstOrDefaultAsync(o => o.Id == request.ReacondicionamientoMecanicoId);

                if (ordenMecanica == null)
                    return NotFound(new ReacondicionamientoVehiculoResponse
                    {
                        Success = false,
                        Message = "Orden de reacondicionamiento mecánico no encontrada"
                    });

                // Verificar que el reacondicionamiento estético existe
                var reacondEstetico = await _db.ReacondicionamientosEsteticos
                    .FirstOrDefaultAsync(r => r.Id == request.ReacondicionamientoEsteticoId && r.Activo);

                if (reacondEstetico == null)
                    return NotFound(new ReacondicionamientoVehiculoResponse
                    {
                        Success = false,
                        Message = "Reacondicionamiento estético no encontrado"
                    });

                var reacond = new ReacondicionamientoVehiculo
                {
                    AvaluoId = request.AvaluoId,
                    VehiculoId = request.VehiculoId,
                    ReacondicionamientoMecanicoId = request.ReacondicionamientoMecanicoId,
                    ReacondicionamientoEsteticoId = request.ReacondicionamientoEsteticoId,
                    FechaCompra = DateTime.Now,
                    TieneReacondicionamientoMecanico = request.TieneReacondicionamientoMecanico,
                    TieneReacondicionamientoEstetico = request.TieneReacondicionamientoEstetico,
                    CostoReacondicionamientoMecanico = request.CostoReacondicionamientoMecanico,
                    CostoReacondicionamientoEstetico = request.CostoReacondicionamientoEstetico
                };

                _db.ReacondicionamientosVehiculos.Add(reacond);
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Reacondicionamiento general ID {Id} creado para avalúo {AvaluoId}",
                    reacond.Id, request.AvaluoId);

                return Ok(new ReacondicionamientoVehiculoResponse
                {
                    Success = true,
                    Message = "Reacondicionamiento registrado exitosamente",
                    ReacondicionamientoId = reacond.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reacondicionamiento general");
                return StatusCode(500, new ReacondicionamientoVehiculoResponse
                {
                    Success = false,
                    Message = "Error al registrar reacondicionamiento"
                });
            }
        }

        // ============================================
        // GET: api/ReacondicionamientoVehiculo/{id}
        // ============================================
        /// <summary>
        /// Obtener detalle de un reacondicionamiento general por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ReacondicionamientoVehiculoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var reacond = await _db.ReacondicionamientosVehiculos
                    .Include(r => r.Avaluo)
                    .Include(r => r.Vehiculo)
                    .Include(r => r.ReacondicionamientoMecanico)
                    .Include(r => r.ReacondicionamientoEstetico)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (reacond == null)
                    return NotFound(new ReacondicionamientoVehiculoResponse
                    {
                        Success = false,
                        Message = "Reacondicionamiento no encontrado"
                    });

                return Ok(MapearDto(reacond));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reacondicionamiento {Id}", id);
                return StatusCode(500, new ReacondicionamientoVehiculoResponse
                {
                    Success = false,
                    Message = "Error al obtener reacondicionamiento"
                });
            }
        }

        // ============================================
        // GET: api/ReacondicionamientoVehiculo/avaluo/{avaluoId}
        // ============================================
        /// <summary>
        /// Obtener el reacondicionamiento asociado a un avalúo
        /// </summary>
        [HttpGet("avaluo/{avaluoId}")]
        [ProducesResponseType(typeof(ReacondicionamientoVehiculoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorAvaluo(int avaluoId)
        {
            try
            {
                var reacond = await _db.ReacondicionamientosVehiculos
                    .Include(r => r.Avaluo)
                    .Include(r => r.Vehiculo)
                    .Include(r => r.ReacondicionamientoMecanico)
                    .Include(r => r.ReacondicionamientoEstetico)
                    .FirstOrDefaultAsync(r => r.AvaluoId == avaluoId);

                if (reacond == null)
                    return NotFound(new ReacondicionamientoVehiculoResponse
                    {
                        Success = false,
                        Message = "No se encontró reacondicionamiento para este avalúo"
                    });

                return Ok(MapearDto(reacond));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reacondicionamiento del avalúo {Id}", avaluoId);
                return StatusCode(500, new ReacondicionamientoVehiculoResponse
                {
                    Success = false,
                    Message = "Error al obtener reacondicionamiento"
                });
            }
        }

        // ============================================
        // GET: api/ReacondicionamientoVehiculo/pendientes
        // ============================================
        /// <summary>
        /// Obtener todos los vehículos en proceso de reacondicionamiento (aún no listos para venta)
        /// </summary>
        [HttpGet("pendientes")]
        [ProducesResponseType(typeof(ListaReacondicionamientoVehiculoResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerPendientes()
        {
            try
            {
                var lista = await _db.ReacondicionamientosVehiculos
                    //.Include(r => r.Avaluo)
                    .Include(r => r.Vehiculo)
                    //.Include(r => r.ReacondicionamientoMecanico)
                    //.Include(r => r.ReacondicionamientoEstetico)
                    .Where(r => !r.VehiculoListoVenta)
                    .OrderBy(r => r.FechaCompra)
                    .ToListAsync();

                return Ok(new ListaReacondicionamientoVehiculoResponse
                {
                    Success = true,
                    Message = lista.Any()
                        ? $"Se encontraron {lista.Count} vehículo(s) en reacondicionamiento"
                        : "Sin vehículos en reacondicionamiento",
                    Reacondicionamientos = lista.Select(MapearDto).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reacondicionamientos pendientes");
                return StatusCode(500, new ListaReacondicionamientoVehiculoResponse
                {
                    Success = false,
                    Message = "Error al obtener reacondicionamientos"
                });
            }
        }

        // ============================================
        // GET: api/ReacondicionamientoVehiculo/pendientes-clasificados/{clasificacion}
        // ============================================
        /// <summary>
        /// Obtener todos los vehículos en proceso de reacondicionamiento (aún no listos para venta)
        /// </summary>
        [HttpGet("pendientes-clasificados/{clasificacion}")]
        [ProducesResponseType(typeof(ListaReacondicionamientoVehiculoResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerPendientesClasificados(string clasificacion)
        {
            try
            {
                var query = _db.ReacondicionamientosVehiculos
                    .Include(r => r.Vehiculo)
                    .Where(r => !r.VehiculoListoVenta);

                switch (clasificacion.ToLower())
                {
                    case "mecanico":
                        query = query.Where(r => r.ReacondicionamientoMecanicoEnProceso);
                        break;

                    case "estetico":
                        query = query.Where(r => r.ReacondicionamientoEsteticoEnProceso);
                        break;

                    case "fotografias":
                        query = query.Where(r => r.VehiculoEnTomaFotografias);
                        break;

                    case "todos":
                        // no se agrega filtro extra
                        break;

                    default:
                        return BadRequest(new ListaReacondicionamientoVehiculoResponse
                        {
                            Success = false,
                            Message = "Clasificación no válida. Usa: todos, mecanico, estetico, fotografias"
                        });
                }

                var lista = await query
                    .OrderBy(r => r.FechaCompra)
                    .ToListAsync();

                return Ok(new ListaReacondicionamientoVehiculoResponse
                {
                    Success = true,
                    Message = lista.Any()
                        ? $"Se encontraron {lista.Count} vehículo(s)"
                        : "Sin vehículos en reacondicionamiento",
                    Reacondicionamientos = lista.Select(MapearDto).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reacondicionamientos pendientes");
                return StatusCode(500, new ListaReacondicionamientoVehiculoResponse
                {
                    Success = false,
                    Message = "Error al obtener reacondicionamientos"
                });
            }
        }


        // ============================================
        // GET: api/ReacondicionamientoVehiculo/ReacondicionamientoMecanicoProceso
        // ============================================
        /// <summary>
        /// Obtener todos los vehículos en proceso de reacondicionamiento mecanico
        /// </summary>
        [HttpGet("ReacondicionamientoMecanicoProceso")]
        [ProducesResponseType(typeof(ListaReacondicionmientosMecanicosEnProceso), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerReacondicionamientosMecanicosEnProceso()
        {
            try
            {
                var lista = await _db.ReacondicionamientosVehiculos
                    .Where(r => !r.VehiculoListoVenta
                        && r.ReacondicionamientoMecanicoEnProceso
                        && !r.TieneReacondicionamientoMecanico)
                    .OrderBy(r => r.FechaCompra)
                    .Select(r => new ReacondicionamientoMecanicoEnProceso
                    {
                        Id = r.Id,
                        VehiculoId = r.VehiculoId,
                        AvaluoId = r.AvaluoId,
                        VehiculoInfo = r.Vehiculo != null
                            ? r.Vehiculo.Marca + " " + r.Vehiculo.Modelo + " " + r.Vehiculo.Anio
                            : "",
                        VIN = r.Vehiculo != null ? r.Vehiculo.VIN : "",
                        OrdenReacondicionamientoId = r.ReacondicionamientoMecanicoId,
                        FechaInicioReacondicionamiento = r.FechaInicioReacondicionamientoMecanico,
                    })
                    .ToListAsync();

                return Ok(new ListaReacondicionmientosMecanicosEnProceso
                {
                    Success = true,
                    Message = lista.Any()
                        ? $"Se encontraron {lista.Count} vehículo(s) en reacondicionamiento"
                        : "Sin vehículos en reacondicionamiento",
                    Reacondicionamientos = lista
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reacondicionamientos pendientes");
                return StatusCode(500, new ListaReacondicionmientosMecanicosEnProceso
                {
                    Success = false,
                    Message = "Error al obtener reacondicionamientos"
                });
            }
        }


        // ============================================
        // GET: api/ReacondicionamientoVehiculo/listos-venta
        // ============================================
        /// <summary>
        /// Obtener vehículos ya listos para la venta
        /// </summary>
        [HttpGet("listos-venta")]
        [ProducesResponseType(typeof(ListaReacondicionamientoVehiculoResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerListosVenta()
        {
            try
            {
                var lista = await _db.ReacondicionamientosVehiculos
                    .Include(r => r.Avaluo)
                    .Include(r => r.Vehiculo)
                    .Include(r => r.ReacondicionamientoMecanico)
                    .Include(r => r.ReacondicionamientoEstetico)
                    .Where(r => r.VehiculoListoVenta && r.FechaLiberacionVehiculo == null)
                    .OrderByDescending(r => r.FechaVehiculoListo)
                    .ToListAsync();

                return Ok(new ListaReacondicionamientoVehiculoResponse
                {
                    Success = true,
                    Message = lista.Any()
                        ? $"Se encontraron {lista.Count} vehículo(s) listos para venta"
                        : "Sin vehículos listos para venta",
                    Reacondicionamientos = lista.Select(MapearDto).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículos listos para venta");
                return StatusCode(500, new ListaReacondicionamientoVehiculoResponse
                {
                    Success = false,
                    Message = "Error al obtener vehículos"
                });
            }
        }

        // ============================================
        // PUT: api/ReacondicionamientoVehiculo/{id}/etapa
        // ============================================
        /// <summary>
        /// Registrar inicio o fin de una etapa del reacondicionamiento
        /// (mecanico, estetico, fotos, listo)
        /// </summary>
        [HttpPut("{id}/etapa")]
        [ProducesResponseType(typeof(ReacondicionamientoVehiculoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActualizarEtapa(
            int id,
            [FromBody] ActualizarEtapaReacondicionamientoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ReacondicionamientoVehiculoResponse
                {
                    Success = false,
                    Message = "Datos inválidos"
                });

            try
            {
                var reacond = await _db.ReacondicionamientosVehiculos.FindAsync(id);

                if (reacond == null)
                    return NotFound(new ReacondicionamientoVehiculoResponse
                    {
                        Success = false,
                        Message = "Reacondicionamiento no encontrado"
                    });

                var ahora = DateTime.Now;

                switch (request.Etapa.ToLower())
                {
                    case "mecanico":
                        if (request.Inicio)
                        {
                            reacond.FechaInicioReacondicionamientoMecanico = ahora;
                            reacond.ReacondicionamientoMecanicoEnProceso = true;
                        }                        
                        else
                        {
                            reacond.FechaFinalizacionReacondicionamientoMecanico = ahora;
                            reacond.ReacondicionamientoMecanicoEnProceso = false;
                            reacond.TieneReacondicionamientoMecanico = true;
                        }

                        break;

                    case "estetico":
                        if (request.Inicio)
                        {
                            reacond.FechaInicioReacondicionamientoEstetico = ahora;
                            reacond.ReacondicionamientoEsteticoEnProceso = true;
                        }  
                        else
                        {
                            reacond.FechaFinalizacionReacondicionamientoEstetico = ahora;
                            reacond.ReacondicionamientoEsteticoEnProceso = false;
                            reacond.TieneReacondicionamientoEstetico = true;
                        }

                        break;

                    case "fotos":
                        if (request.Inicio)
                        {
                            reacond.FechaInicioTomaFotografias = ahora;
                            reacond.VehiculoEnTomaFotografias = true;
                        }
 
                        else
                        {
                            reacond.VehiculoEnTomaFotografias = false;
                            reacond.FechaFinalizacionTomaFotografias = ahora;
                            reacond.TieneFotografias = true;
                        }
  
                        break;

                    case "listo":
                        reacond.VehiculoListoVenta = true;
                        reacond.FechaVehiculoListo = ahora;
                break;
                }

                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Etapa '{Etapa}' actualizada en reacondicionamiento {Id}", request.Etapa, id);

                return Ok(new ReacondicionamientoVehiculoResponse
                {
                    Success = true,
                    Message = $"Etapa '{request.Etapa}' actualizada exitosamente",
                    ReacondicionamientoId = id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar etapa del reacondicionamiento {Id}", id);
                return StatusCode(500, new ReacondicionamientoVehiculoResponse
                {
                    Success = false,
                    Message = "Error al actualizar etapa"
                });
            }
        }

        // ============================================
        // PUT: api/ReacondicionamientoVehiculo/{id}/liberar
        // ============================================
        /// <summary>
        /// Registrar la liberación/entrega del vehículo al área de ventas
        /// </summary>
        [HttpPut("{id}/liberar")]
        [ProducesResponseType(typeof(ReacondicionamientoVehiculoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LiberarVehiculo(
            int id,
            [FromBody] LiberarVehiculoRequest request)
        {
            try
            {
                var reacond = await _db.ReacondicionamientosVehiculos.FindAsync(id);

                if (reacond == null)
                    return NotFound(new ReacondicionamientoVehiculoResponse
                    {
                        Success = false,
                        Message = "Reacondicionamiento no encontrado"
                    });

                if (!reacond.VehiculoListoVenta)
                    return BadRequest(new ReacondicionamientoVehiculoResponse
                    {
                        Success = false,
                        Message = "El vehículo aún no ha sido marcado como listo para venta"
                    });

                reacond.FechaLiberacionVehiculo = request.FechaLiberacion ?? DateTime.Now;

                await _db.SaveChangesAsync();

                _logger.LogInformation("Vehículo liberado en reacondicionamiento {Id}", id);

                return Ok(new ReacondicionamientoVehiculoResponse
                {
                    Success = true,
                    Message = "Vehículo liberado al área de ventas exitosamente",
                    ReacondicionamientoId = id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al liberar vehículo del reacondicionamiento {Id}", id);
                return StatusCode(500, new ReacondicionamientoVehiculoResponse
                {
                    Success = false,
                    Message = "Error al liberar vehículo"
                });
            }
        }

        // ============================================
        // MÉTODO PRIVADO — Mapeo a DTO
        // ============================================
        private static ReacondicionamientoVehiculoDto MapearDto(ReacondicionamientoVehiculo r) => new()
        {
            Id = r.Id,
            AvaluoId = r.AvaluoId,
            VehiculoInfo = r.Vehiculo != null
                ? $"{r.Vehiculo.Marca} {r.Vehiculo.Modelo} {r.Vehiculo.Anio} {r.Vehiculo.Color}"
                : "",
            VIN = r.Vehiculo?.VIN ?? "",
            FechaCompra = r.FechaCompra,

            // Mecánico
            TieneReacondicionamientoMecanico = r.TieneReacondicionamientoMecanico,
            FechaInicioReacondicionamientoMecanico = r.FechaInicioReacondicionamientoMecanico,
            FechaFinalizacionReacondicionamientoMecanico = r.FechaFinalizacionReacondicionamientoMecanico,
            CostoReacondicionamientoMecanico = r.CostoReacondicionamientoMecanico,
            //ReacondicionamientoMecanicoId = r.ReacondicionamientoMecanicoId,
            //NumeroOrdenMecanica = r.ReacondicionamientoMecanico?.NumeroOrden,

            // Estético
            TieneReacondicionamientoEstetico = r.TieneReacondicionamientoEstetico,
            FechaInicioReacondicionamientoEstetico = r.FechaInicioReacondicionamientoEstetico,
            FechaFinalizacionReacondicionamientoEstetico = r.FechaFinalizacionReacondicionamientoEstetico,
            CostoReacondicionamientoEstetico = r.CostoReacondicionamientoEstetico,
            //ReacondicionamientoEsteticoId = r.ReacondicionamientoEsteticoId,
            //ProgresoEstetico = r.ReacondicionamientoEstetico?.ProgresoGeneral ?? 0,

            // Fotos
            TieneFotografias = r.TieneFotografias,
            FechaInicioTomaFotografias = r.FechaInicioTomaFotografias,
            FechaFinalizacionTomaFotografias = r.FechaFinalizacionTomaFotografias,

            // Final
            VehiculoListoVenta = r.VehiculoListoVenta,
            FechaVehiculoListo = r.FechaVehiculoListo,
            FechaLiberacionVehiculo = r.FechaLiberacionVehiculo,
            ///
            ReacondicionamientoEsteticoEnProceso = r.ReacondicionamientoEsteticoEnProceso,
            ReacondicionamientoMecanicoEnProceso = r.ReacondicionamientoMecanicoEnProceso,
            VehiculoEnTomaFotografias =r.VehiculoEnTomaFotografias,

            // Total (columna generada en DB)
            CostoTotalReacondicionamiento = r.CostoTotalReacondicionamiento
        };
    }
}