// ============================================
// Controllers/ReacondicionamientoEsteticoController.cs
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
    public class ReacondicionamientoEsteticoController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ReacondicionamientoEsteticoController> _logger;

        public ReacondicionamientoEsteticoController(
            ApplicationDbContext db,
            ILogger<ReacondicionamientoEsteticoController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ============================================
        // POST: api/ReacondicionamientoEstetico/crear
        // ============================================
        /// <summary>
        /// Crear un nuevo reacondicionamiento estético con sus trabajos iniciales
        /// </summary>
        [HttpPost("crear")]
        [ProducesResponseType(typeof(ReacondicionamientoEsteticoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Crear([FromBody] CrearReacondicionamientoEsteticoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ReacondicionamientoEsteticoResponse
                {
                    Success = false,
                    Message = "Datos inválidos: " + string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });

            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    // Verificar que el encargado existe
                    var encargado = await _db.Usuarios
                        .FirstOrDefaultAsync(u => u.Id == request.EncargadoEsteticaId && u.Activo);

                    if (encargado == null)
                        return BadRequest(new ReacondicionamientoEsteticoResponse
                        {
                            Success = false,
                            Message = "El encargado no existe o no está activo"
                        });

                    // Verificar que el vehículo existe
                    var vehiculo = await _db.Vehiculos
                        .FirstOrDefaultAsync(v => v.Id == request.VehiculoId && v.Activo);

                    if (vehiculo == null)
                        return NotFound(new ReacondicionamientoEsteticoResponse
                        {
                            Success = false,
                            Message = "Vehículo no encontrado"
                        });

                    // Crear el reacondicionamiento estético
                    var reacond = new ReacondicionamientoEstetico
                    {
                        EncargadoEsteticaId = request.EncargadoEsteticaId,
                        VehiculoId = request.VehiculoId,
                        EstadoOrdenId = 1,
                        TotalTrabajos = request.Trabajos.Count,
                        TrabajosCompletados = 0,
                        ProgresoGeneral = 0.00m,
                        CostoTotal = 0.00m,
                        Activo = true
                    };

                    _db.ReacondicionamientosEsteticos.Add(reacond);
                    await _db.SaveChangesAsync();

                    // Agregar los trabajos
                    foreach (var t in request.Trabajos)
                    {
                        var trabajo = new TrabajoReacondicionamientoEstetico
                        {
                            ReacondicionamientoEsteticoId = reacond.Id,
                            Trabajo = t.Trabajo,
                            EmpresaQueRealizara = t.EmpresaQueRealizara,
                            IndicacionesTrabajo = t.IndicacionesTrabajo,
                            CostoTrabajo = t.CostoTrabajo,
                            EstadoTrabajo = 1,
                            Activo = true
                        };
                        _db.TrabajosReacondicionamientoEstetico.Add(trabajo);
                    }

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation(
                        "Reacondicionamiento estético ID {Id} creado para vehículo {VehiculoId}",
                        reacond.Id, request.VehiculoId);

                    return Ok(new ReacondicionamientoEsteticoResponse
                    {
                        Success = true,
                        Message = "Reacondicionamiento estético creado exitosamente",
                        ReacondicionamientoId = reacond.Id
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al crear reacondicionamiento estético");
                    return StatusCode(500, new ReacondicionamientoEsteticoResponse
                    {
                        Success = false,
                        Message = "Error al crear reacondicionamiento estético"
                    });
                }
            });
        }

        // ============================================
        // GET: api/ReacondicionamientoEstetico/{id}
        // ============================================
        /// <summary>
        /// Obtener detalle completo de un reacondicionamiento estético con sus trabajos
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ReacondicionamientoEsteticoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var reacond = await _db.ReacondicionamientosEsteticos
                    .Include(r => r.EncargadoEstetica)
                    .Include(r => r.Vehiculo)
                    .Include(r => r.EstadoOrden)
                    .Include(r => r.Trabajos.Where(t => t.Activo))
                    .Where(r => r.Id == id && r.Activo)
                    .FirstOrDefaultAsync();

                if (reacond == null)
                    return NotFound(new ReacondicionamientoEsteticoResponse
                    {
                        Success = false,
                        Message = "Reacondicionamiento estético no encontrado"
                    });

                return Ok(MapearDto(reacond));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reacondicionamiento estético {Id}", id);
                return StatusCode(500, new ReacondicionamientoEsteticoResponse
                {
                    Success = false,
                    Message = "Error al obtener reacondicionamiento estético"
                });
            }
        }

        // ============================================
        // GET: api/ReacondicionamientoEstetico/vehiculo/{vehiculoId}
        // ============================================
        /// <summary>
        /// Obtener reacondicionamientos estéticos de un vehículo
        /// </summary>
        [HttpGet("vehiculo/{vehiculoId}")]
        [ProducesResponseType(typeof(ListaReacondicionamientoEsteticoResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerPorVehiculo(int vehiculoId)
        {
            try
            {
                var lista = await _db.ReacondicionamientosEsteticos
                    .Include(r => r.EncargadoEstetica)
                    .Include(r => r.Vehiculo)
                    .Include(r => r.EstadoOrden)
                    .Include(r => r.Trabajos.Where(t => t.Activo))
                    .Where(r => r.VehiculoId == vehiculoId && r.Activo)
                    .OrderByDescending(r => r.FechaCreacion)
                    .ToListAsync();

                return Ok(new ListaReacondicionamientoEsteticoResponse
                {
                    Success = true,
                    Message = lista.Any()
                        ? $"Se encontraron {lista.Count} reacondicionamiento(s)"
                        : "Sin reacondicionamientos registrados",
                    Reacondicionamientos = lista.Select(MapearDto).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reacondicionamientos del vehículo {Id}", vehiculoId);
                return StatusCode(500, new ListaReacondicionamientoEsteticoResponse
                {
                    Success = false,
                    Message = "Error al obtener reacondicionamientos"
                });
            }
        }

        // ============================================
        // GET: api/ReacondicionamientoEstetico/activos
        // ============================================
        /// <summary>
        /// Obtener todos los reacondicionamientos estéticos activos (en progreso)
        /// </summary>
        [HttpGet("activos")]
        [ProducesResponseType(typeof(ListaReacondicionamientoEsteticoResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerActivos()
        {
            try
            {
                var lista = await _db.ReacondicionamientosEsteticos
                    .Include(r => r.EncargadoEstetica)
                    .Include(r => r.Vehiculo)
                    .Include(r => r.EstadoOrden)
                    .Include(r => r.Trabajos.Where(t => t.Activo))
                    .Where(r => r.Activo && r.EstadoOrdenId < 3) // 1=Pendiente 2=En proceso
                    .OrderBy(r => r.FechaCreacion)
                    .ToListAsync();

                return Ok(new ListaReacondicionamientoEsteticoResponse
                {
                    Success = true,
                    Message = lista.Any()
                        ? $"Se encontraron {lista.Count} reacondicionamiento(s) activos"
                        : "Sin reacondicionamientos activos",
                    Reacondicionamientos = lista.Select(MapearDto).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reacondicionamientos activos");
                return StatusCode(500, new ListaReacondicionamientoEsteticoResponse
                {
                    Success = false,
                    Message = "Error al obtener reacondicionamientos"
                });
            }
        }

        // ============================================
        // POST: api/ReacondicionamientoEstetico/agregar-trabajo
        // ============================================
        /// <summary>
        /// Agregar un trabajo a un reacondicionamiento estético existente
        /// </summary>
        [HttpPost("agregar-trabajo")]
        [ProducesResponseType(typeof(ReacondicionamientoEsteticoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AgregarTrabajo([FromBody] AgregarTrabajoEsteticoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ReacondicionamientoEsteticoResponse
                {
                    Success = false,
                    Message = "Datos inválidos"
                });

            try
            {
                var reacond = await _db.ReacondicionamientosEsteticos
                    .FirstOrDefaultAsync(r => r.Id == request.ReacondicionamientoEsteticoId && r.Activo);

                if (reacond == null)
                    return NotFound(new ReacondicionamientoEsteticoResponse
                    {
                        Success = false,
                        Message = "Reacondicionamiento estético no encontrado"
                    });

                var trabajo = new TrabajoReacondicionamientoEstetico
                {
                    ReacondicionamientoEsteticoId = request.ReacondicionamientoEsteticoId,
                    Trabajo = request.Trabajo,
                    EmpresaQueRealizara = request.EmpresaQueRealizara,
                    IndicacionesTrabajo = request.IndicacionesTrabajo,
                    CostoTrabajo = request.CostoTrabajo,
                    EstadoTrabajo = 1,
                    Activo = true
                };

                _db.TrabajosReacondicionamientoEstetico.Add(trabajo);
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Trabajo agregado al reacondicionamiento estético {Id}", request.ReacondicionamientoEsteticoId);

                return Ok(new ReacondicionamientoEsteticoResponse
                {
                    Success = true,
                    Message = "Trabajo agregado exitosamente",
                    ReacondicionamientoId = reacond.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar trabajo al reacondicionamiento estético");
                return StatusCode(500, new ReacondicionamientoEsteticoResponse
                {
                    Success = false,
                    Message = "Error al agregar trabajo"
                });
            }
        }

        // ============================================
        // PUT: api/ReacondicionamientoEstetico/trabajo/{trabajoId}/estado
        // ============================================
        /// <summary>
        /// Cambiar estado de un trabajo estético (1=Pendiente, 2=En Proceso, 3=Completado)
        /// </summary>
        [HttpPut("trabajo/{trabajoId}/estado")]
        [ProducesResponseType(typeof(ReacondicionamientoEsteticoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CambiarEstadoTrabajo(
            int trabajoId,
            [FromBody] CambiarEstadoTrabajoEsteticoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ReacondicionamientoEsteticoResponse
                {
                    Success = false,
                    Message = "Estado inválido. Debe ser 1, 2 o 3"
                });

            try
            {
                var trabajo = await _db.TrabajosReacondicionamientoEstetico
                    .FirstOrDefaultAsync(t => t.Id == trabajoId && t.Activo);

                if (trabajo == null)
                    return NotFound(new ReacondicionamientoEsteticoResponse
                    {
                        Success = false,
                        Message = "Trabajo no encontrado"
                    });

                // Registrar fechas según el estado
                if (request.NuevoEstado == 2 && trabajo.FechaHoraInicio == null)
                    trabajo.FechaHoraInicio = DateTime.Now;
                else if (request.NuevoEstado == 3)
                    trabajo.FechaHoraTermino = DateTime.Now;

                trabajo.EstadoTrabajo = request.NuevoEstado;
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Trabajo {TrabajoId} cambió a estado {Estado}", trabajoId, request.NuevoEstado);

                return Ok(new ReacondicionamientoEsteticoResponse
                {
                    Success = true,
                    Message = "Estado del trabajo actualizado exitosamente",
                    ReacondicionamientoId = trabajo.ReacondicionamientoEsteticoId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del trabajo {Id}", trabajoId);
                return StatusCode(500, new ReacondicionamientoEsteticoResponse
                {
                    Success = false,
                    Message = "Error al actualizar estado"
                });
            }
        }

        // ============================================
        // PUT: api/ReacondicionamientoEstetico/trabajo/{trabajoId}
        // ============================================
        /// <summary>
        /// Actualizar datos de un trabajo estético (empresa, indicaciones, costo)
        /// </summary>
        [HttpPut("trabajo/{trabajoId}")]
        [ProducesResponseType(typeof(ReacondicionamientoEsteticoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActualizarTrabajo(
            int trabajoId,
            [FromBody] ActualizarTrabajoEsteticoRequest request)
        {
            try
            {
                var trabajo = await _db.TrabajosReacondicionamientoEstetico
                    .FirstOrDefaultAsync(t => t.Id == trabajoId && t.Activo);

                if (trabajo == null)
                    return NotFound(new ReacondicionamientoEsteticoResponse
                    {
                        Success = false,
                        Message = "Trabajo no encontrado"
                    });

                if (request.EmpresaQueRealizara != null)
                    trabajo.EmpresaQueRealizara = request.EmpresaQueRealizara;

                if (request.IndicacionesTrabajo != null)
                    trabajo.IndicacionesTrabajo = request.IndicacionesTrabajo;

                if (request.CostoTrabajo.HasValue)
                    trabajo.CostoTrabajo = request.CostoTrabajo.Value;

                await _db.SaveChangesAsync();

                return Ok(new ReacondicionamientoEsteticoResponse
                {
                    Success = true,
                    Message = "Trabajo actualizado exitosamente",
                    ReacondicionamientoId = trabajo.ReacondicionamientoEsteticoId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar trabajo {Id}", trabajoId);
                return StatusCode(500, new ReacondicionamientoEsteticoResponse
                {
                    Success = false,
                    Message = "Error al actualizar trabajo"
                });
            }
        }

        // ============================================
        // PUT: api/ReacondicionamientoEstetico/{id}/finalizar
        // ============================================
        /// <summary>
        /// Marcar el reacondicionamiento estético como finalizado
        /// </summary>
        [HttpPut("{id}/finalizar")]
        [ProducesResponseType(typeof(ReacondicionamientoEsteticoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Finalizar(int id)
        {
            try
            {
                var reacond = await _db.ReacondicionamientosEsteticos
                    .Include(r => r.Trabajos)
                    .FirstOrDefaultAsync(r => r.Id == id && r.Activo);

                if (reacond == null)
                    return NotFound(new ReacondicionamientoEsteticoResponse
                    {
                        Success = false,
                        Message = "Reacondicionamiento estético no encontrado"
                    });

                // Verificar que todos los trabajos activos estén completados
                var trabajosPendientes = reacond.Trabajos.Count(t => t.Activo && t.EstadoTrabajo != 3);
                if (trabajosPendientes > 0)
                    return BadRequest(new ReacondicionamientoEsteticoResponse
                    {
                        Success = false,
                        Message = $"Hay {trabajosPendientes} trabajo(s) sin completar"
                    });

                reacond.EstadoOrdenId = 3; // Finalizada
                reacond.FechaFinalizacion = DateTime.Now;

                await _db.SaveChangesAsync();

                return Ok(new ReacondicionamientoEsteticoResponse
                {
                    Success = true,
                    Message = "Reacondicionamiento estético finalizado exitosamente",
                    ReacondicionamientoId = id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al finalizar reacondicionamiento estético {Id}", id);
                return StatusCode(500, new ReacondicionamientoEsteticoResponse
                {
                    Success = false,
                    Message = "Error al finalizar reacondicionamiento"
                });
            }
        }

        // ============================================
        // DELETE: api/ReacondicionamientoEstetico/trabajo/{trabajoId}
        // ============================================
        /// <summary>
        /// Eliminar (desactivar) un trabajo estético
        /// </summary>
        [HttpDelete("trabajo/{trabajoId}")]
        [ProducesResponseType(typeof(ReacondicionamientoEsteticoResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> EliminarTrabajo(int trabajoId)
        {
            try
            {
                var trabajo = await _db.TrabajosReacondicionamientoEstetico
                    .FirstOrDefaultAsync(t => t.Id == trabajoId && t.Activo);

                if (trabajo == null)
                    return NotFound(new ReacondicionamientoEsteticoResponse
                    {
                        Success = false,
                        Message = "Trabajo no encontrado"
                    });

                if (trabajo.EstadoTrabajo == 3)
                    return BadRequest(new ReacondicionamientoEsteticoResponse
                    {
                        Success = false,
                        Message = "No se puede eliminar un trabajo ya completado"
                    });

                trabajo.Activo = false;
                await _db.SaveChangesAsync();

                return Ok(new ReacondicionamientoEsteticoResponse
                {
                    Success = true,
                    Message = "Trabajo eliminado exitosamente",
                    ReacondicionamientoId = trabajo.ReacondicionamientoEsteticoId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar trabajo {Id}", trabajoId);
                return StatusCode(500, new ReacondicionamientoEsteticoResponse
                {
                    Success = false,
                    Message = "Error al eliminar trabajo"
                });
            }
        }

        // ============================================
        // MÉTODO PRIVADO — Mapeo a DTO
        // ============================================
        private static ReacondicionamientoEsteticoDto MapearDto(ReacondicionamientoEstetico r) => new()
        {
            Id = r.Id,
            EncargadoEsteticaId = r.EncargadoEsteticaId,
            EncargadoNombre = r.EncargadoEstetica?.NombreCompleto ?? "",
            VehiculoId = r.VehiculoId,
            VehiculoInfo = r.Vehiculo != null
                ? $"{r.Vehiculo.Marca} {r.Vehiculo.Modelo} {r.Vehiculo.Anio}"
                : "",
            FechaCreacion = r.FechaCreacion,
            FechaFinalizacion = r.FechaFinalizacion,
            EstadoOrdenId = r.EstadoOrdenId,
            EstadoOrden = r.EstadoOrden?.NombreEstado ?? "",
            CostoTotal = r.CostoTotal,
            TotalTrabajos = r.TotalTrabajos,
            TrabajosCompletados = r.TrabajosCompletados,
            ProgresoGeneral = r.ProgresoGeneral,
            Activo = r.Activo,
            Trabajos = r.Trabajos.Select(t => new TrabajoEsteticoDto
            {
                Id = t.Id,
                ReacondicionamientoEsteticoId = t.ReacondicionamientoEsteticoId,
                Trabajo = t.Trabajo,
                EmpresaQueRealizara = t.EmpresaQueRealizara,
                FechaHoraInicio = t.FechaHoraInicio,
                FechaHoraTermino = t.FechaHoraTermino,
                IndicacionesTrabajo = t.IndicacionesTrabajo,
                EstadoTrabajo = t.EstadoTrabajo,
                CostoTrabajo = t.CostoTrabajo,
                Activo = t.Activo
            }).ToList()
        };
    }
}