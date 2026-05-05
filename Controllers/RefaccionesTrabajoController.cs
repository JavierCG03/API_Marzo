using CarSlineAPI.Data;
using CarSlineAPI.Models.DTOs;
using CarSlineAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarSlineAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RefaccionesTrabajoController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<RefaccionesTrabajoController> _logger;

        public RefaccionesTrabajoController(ApplicationDbContext db, ILogger<RefaccionesTrabajoController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpPost("agregar")]
        [ProducesResponseType(typeof(AgregarRefaccionesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AgregarRefacciones([FromBody] AgregarRefaccionesTrabajoRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AgregarRefaccionesResponse
                {
                    Success = false,
                    Message = "Datos inválidos"
                });
            }

            try
            {
                // Verificar que el trabajo existe
                var trabajo = await _db.TrabajosPorOrden
                    .Include(t => t.OrdenGeneral)
                    .FirstOrDefaultAsync(t => t.Id == request.TrabajoId && t.Activo);

                if (trabajo == null)
                {
                    return NotFound(new AgregarRefaccionesResponse
                    {
                        Success = false,
                        Message = "Trabajo no encontrado"
                    });
                }

                // Validar que el trabajo no esté completado o cancelado
                if (trabajo.EstadoTrabajo == 6)
                {
                    return BadRequest(new AgregarRefaccionesResponse
                    {
                        Success = false,
                        Message = "No se pueden agregar refacciones a un trabajo cancelado"
                    });
                }

                var refaccionesAgregadas = new List<RefaccionTrabajoDto>();
                decimal totalRefacciones = 0;

                // Procesar cada refacción
                foreach (var refaccionDto in request.Refacciones)
                {
                    var total = refaccionDto.Cantidad * refaccionDto.PrecioUnitario;
                    totalRefacciones += total;

                    var refaccionTrabajo = new Refacciontrabajo
                    {
                        TrabajoId = request.TrabajoId,
                        OrdenGeneralId = trabajo.OrdenGeneralId,
                        Refaccion = refaccionDto.Refaccion,
                        Cantidad = refaccionDto.Cantidad,
                        PrecioUnitario = refaccionDto.PrecioUnitario
                    };

                    _db.Set<Refacciontrabajo>().Add(refaccionTrabajo);

                    refaccionesAgregadas.Add(new RefaccionTrabajoDto
                    {
                        Id = 0, // Se asignará después del SaveChanges
                        TrabajoId = refaccionTrabajo.TrabajoId,
                        OrdenGeneralId = refaccionTrabajo.OrdenGeneralId,
                        Refaccion = refaccionTrabajo.Refaccion,
                        Cantidad = refaccionTrabajo.Cantidad,
                        PrecioUnitario = refaccionTrabajo.PrecioUnitario,
                    });
                }


                await _db.SaveChangesAsync();


                await _db.Entry(trabajo).ReloadAsync();

                // Actualizar los IDs después de guardar
                var refaccionesGuardadas = await _db.Set<Refacciontrabajo>()
                    .Where(r => r.TrabajoId == request.TrabajoId)
                    .OrderByDescending(r => r.Id)
                    .Take(request.Refacciones.Count)
                    .ToListAsync();

                for (int i = 0; i < refaccionesAgregadas.Count && i < refaccionesGuardadas.Count; i++)
                {
                    refaccionesAgregadas[i].Id = refaccionesGuardadas[i].Id;
                }

                _logger.LogInformation(
                    $"Se agregaron {refaccionesAgregadas.Count} refacciones al trabajo {request.TrabajoId}. " +
                    $"Total calculado por trigger: ${trabajo.RefaccionesTotal:F2}");

                return Ok(new AgregarRefaccionesResponse
                {
                    Success = true,
                    Message = $"Se agregaron {refaccionesAgregadas.Count} refacción(es) exitosamente",
                    RefaccionesAgregadas = refaccionesAgregadas,
                    TotalRefacciones = trabajo.RefaccionesTotal, // ✅ Usar el valor actualizado por el trigger
                    CantidadRefacciones = refaccionesAgregadas.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al agregar refacciones al trabajo {request.TrabajoId}");
                return StatusCode(500, new AgregarRefaccionesResponse
                {
                    Success = false,
                    Message = "Error al agregar refacciones"
                });
            }
        }

        /// <summary>
        /// Transferir refacción comprada a refacción de trabajo
        /// POST api/RefaccionesTrabajo/transferir
        /// </summary>
        [HttpPost("transferir")]
        [ProducesResponseType(typeof(AgregarRefaccionesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> TransferirRefaccionComprada([FromBody] TransferirRefaccionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new AgregarRefaccionesResponse { Success = false, Message = "Datos inválidos" });

            try
            {
                var refaccionComprada = await _db.RefaccionesCompradas
                    .FirstOrDefaultAsync(r => r.Id == request.RefaccionCompradaId);

                if (refaccionComprada == null)
                    return NotFound(new AgregarRefaccionesResponse { Success = false, Message = "Refacción comprada no encontrada" });

                if (refaccionComprada.Activo)
                    return BadRequest(new AgregarRefaccionesResponse { Success = false, Message = "Esta refacción ya fue transferida a un trabajo" });

                if (!refaccionComprada.TrabajoOrdenId.HasValue)
                    return BadRequest(new AgregarRefaccionesResponse { Success = false, Message = "La refacción no está vinculada a ningún trabajo de orden. Primero convierta la cita en orden." });

                var trabajo = await _db.TrabajosPorOrden
                    .Include(t => t.OrdenGeneral)
                    .FirstOrDefaultAsync(t => t.Id == refaccionComprada.TrabajoOrdenId.Value && t.Activo);

                if (trabajo == null)
                    return NotFound(new AgregarRefaccionesResponse { Success = false, Message = "El trabajo de orden vinculado no fue encontrado o está inactivo" });

                if (trabajo.EstadoTrabajo == 6)
                    return BadRequest(new AgregarRefaccionesResponse { Success = false, Message = "No se pueden transferir refacciones a un trabajo cancelado" });

                var refaccionTrabajo = new Refacciontrabajo
                {
                    TrabajoId = refaccionComprada.TrabajoOrdenId.Value,
                    OrdenGeneralId = trabajo.OrdenGeneralId,
                    Refaccion = refaccionComprada.Refaccion,
                    Cantidad = refaccionComprada.Cantidad,
                    PrecioUnitario = request.PrecioVenta
                };

                _db.Set<Refacciontrabajo>().Add(refaccionTrabajo);

                refaccionComprada.PrecioVenta = request.PrecioVenta;
                refaccionComprada.Activo = true;

                await _db.SaveChangesAsync();

                _logger.LogInformation($"Refacción comprada {request.RefaccionCompradaId} transferida al trabajo {refaccionComprada.TrabajoOrdenId}.");

                return Ok(new AgregarRefaccionesResponse { Success = true, Message = "Refacción transferida exitosamente al trabajo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al transferir refacción comprada {request.RefaccionCompradaId}");
                return StatusCode(500, new AgregarRefaccionesResponse { Success = false, Message = "Error al transferir refacción" });
            }
        }
        /// <summary>
        /// Obtener todas las refacciones de un trabajo
        /// GET api/RefaccionesTrabajo/trabajo/{trabajoId}
        /// </summary>
        [HttpGet("trabajo/{trabajoId}")]
        [ProducesResponseType(typeof(ObtenerRefaccionesTrabajoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerRefaccionesPorTrabajo(int trabajoId)
        {
            try
            {
                var trabajo = await _db.TrabajosPorOrden
                    .Include(t => t.OrdenGeneral)
                    .FirstOrDefaultAsync(t => t.Id == trabajoId);

                if (trabajo == null)
                {
                    return NotFound(new ObtenerRefaccionesTrabajoResponse
                    {
                        Success = false,
                        Message = "Trabajo no encontrado"
                    });
                }

                var refacciones = await _db.Set<Refacciontrabajo>()
                    .Where(r => r.TrabajoId == trabajoId)
                    .Select(r => new RefaccionTrabajoDto
                    {
                        Id = r.Id,
                        TrabajoId = r.TrabajoId,
                        OrdenGeneralId = r.OrdenGeneralId,
                        Refaccion = r.Refaccion,
                        Cantidad = r.Cantidad,
                        PrecioUnitario = r.PrecioUnitario,
                        Total = r.Cantidad * r.PrecioUnitario
                    })
                    .ToListAsync();

                var total = refacciones.Sum(r => r.Total);

                return Ok(new ObtenerRefaccionesTrabajoResponse
                {
                    Success = true,
                    Message = refacciones.Any()
                        ? $"Se encontraron {refacciones.Count} refacción(es)"
                        : "No hay refacciones registradas",
                    TrabajoId = trabajoId,
                    NumeroOrden = trabajo.OrdenGeneral?.NumeroOrden ?? "",
                    Refacciones = refacciones,
                    TotalRefacciones = total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener refacciones del trabajo {trabajoId}");
                return StatusCode(500, new ObtenerRefaccionesTrabajoResponse
                {
                    Success = false,
                    Message = "Error al obtener refacciones"
                });
            }
        }

        /// <summary>
        /// Eliminar una refacción específica de un trabajo
        /// DELETE api/RefaccionesTrabajo/{refaccionId}
        /// </summary>
        [HttpDelete("{refaccionId}")]
        public async Task<IActionResult> EliminarRefaccion(int refaccionId)
        {
            try
            {
                var data = await _db.Set<Refacciontrabajo>()
                    .Where(r => r.Id == refaccionId)
                    .Select(r => new
                    {
                        Refaccion = r,
                        r.TrabajoPorOrden.OrdenGeneral.EstadoOrdenId
                    })
                    .FirstOrDefaultAsync();

                if (data == null)
                {
                    return NotFound(new AgregarRefaccionesResponse
                    {
                        Success = false,
                        Message = "Refacción no encontrada"
                    });
                }

                if (data.EstadoOrdenId == 4)
                {
                    return BadRequest(new AgregarRefaccionesResponse
                    {
                        Success = false,
                        Message = "No se pueden eliminar refacciones de una orden entregada"
                    });
                }

                _db.Remove(data.Refaccion);
                await _db.SaveChangesAsync();

                return Ok(new AgregarRefaccionesResponse
                {
                    Success = true,
                    Message = "Refacción eliminada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar refacción {refaccionId}");

                return StatusCode(500, new AgregarRefaccionesResponse
                {
                    Success = false,
                    Message = "Error al eliminar refacción"
                });
            }
        }

        /// <summary>
        /// Obtener todas las refacciones de una orden
        /// GET api/RefaccionesTrabajo/orden/{ordenId}
        /// </summary>
        [HttpGet("orden/{ordenId}")]
        [ProducesResponseType(typeof(ObtenerRefaccionesTrabajoResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerRefaccionesPorOrden(int ordenId)
        {
            try
            {
                var orden = await _db.OrdenesGenerales.FindAsync(ordenId);

                if (orden == null)
                {
                    return NotFound(new ObtenerRefaccionesTrabajoResponse
                    {
                        Success = false,
                        Message = "Orden no encontrada"
                    });
                }

                var refacciones = await _db.Set<Refacciontrabajo>()
                    .Where(r => r.OrdenGeneralId == ordenId)
                    .Select(r => new RefaccionTrabajoDto
                    {
                        Id = r.Id,
                        TrabajoId = r.TrabajoId,
                        OrdenGeneralId = r.OrdenGeneralId,
                        Refaccion = r.Refaccion,
                        Cantidad = r.Cantidad,
                        PrecioUnitario = r.PrecioUnitario,
                        Total = r.Cantidad * r.PrecioUnitario
                    })
                    .ToListAsync();

                var total = refacciones.Sum(r => r.Total);

                return Ok(new ObtenerRefaccionesTrabajoResponse
                {
                    Success = true,
                    Message = refacciones.Any()
                        ? $"Se encontraron {refacciones.Count} refacción(es)"
                        : "No hay refacciones registradas",
                    TrabajoId = 0,
                    NumeroOrden = orden.NumeroOrden,
                    Refacciones = refacciones,
                    TotalRefacciones = total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener refacciones de la orden {ordenId}");
                return StatusCode(500, new ObtenerRefaccionesTrabajoResponse
                {
                    Success = false,
                    Message = "Error al obtener refacciones"
                });
            }
        }

        /// <summary>
        /// Obtener conteo de trabajos con refacciones pendientes por tipo y fecha
        /// GET api/Ordenes/trabajos-citas/conteo-refacciones
        /// </summary>
        [HttpGet("trabajos-citas/conteo-refacciones")]
        [ProducesResponseType(typeof(OrdenesActivasDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerConteoRefaccionesPendientes([FromQuery] DateTime? fecha = null)
        {
            try
            {
                var fechaConsulta = (fecha ?? DateTime.Today).Date;
                var fechaSiguiente = fechaConsulta.AddDays(1);
                var hoy = DateTime.Today;

                var conteos = new List<(int TipoOrdenId, int Total)>();

                if (fechaConsulta > hoy)
                {
                    // Fecha futura → buscar en Citas
                    conteos = await _db.Citas
                        .Where(c => c.Activo
                                 && c.FechaCita >= fechaConsulta
                                 && c.FechaCita < fechaSiguiente)
                        .SelectMany(c => c.Trabajos.Where(t => t.Activo && !t.RefaccionesListas),
                                    (c, t) => new { c.TipoOrdenId })
                        .GroupBy(x => x.TipoOrdenId)
                        .Select(g => new { TipoOrdenId = g.Key, Total = g.Count() })
                        .ToListAsync()
                        .ContinueWith(t => t.Result.Select(x => (x.TipoOrdenId, x.Total)).ToList());
                }
                else if (fechaConsulta < hoy)
                {
                    // Fecha pasada → buscar en Órdenes
                    conteos = await _db.OrdenesGenerales
                        .Where(o => o.Activo
                                 && o.FechaCreacion >= fechaConsulta
                                 && o.FechaCreacion < fechaSiguiente)
                        .SelectMany(o => o.Trabajos.Where(t => t.Activo && !t.RefaccionesListas),
                                    (o, t) => new { o.TipoOrdenId })
                        .GroupBy(x => x.TipoOrdenId)
                        .Select(g => new { TipoOrdenId = g.Key, Total = g.Count() })
                        .ToListAsync()
                        .ContinueWith(t => t.Result.Select(x => (x.TipoOrdenId, x.Total)).ToList());
                }
                else
                {
                    // Hoy → Citas activas aún no convertidas + Órdenes activas (EstadoOrdenId < 3)
                    var conteosCitas = await _db.Citas
                        .Where(c => c.Activo
                                 && c.FechaCita >= fechaConsulta
                                 && c.FechaCita < fechaSiguiente)
                        .SelectMany(c => c.Trabajos.Where(t => t.Activo && !t.RefaccionesListas),
                                    (c, t) => new { c.TipoOrdenId })
                        .GroupBy(x => x.TipoOrdenId)
                        .Select(g => new { TipoOrdenId = g.Key, Total = g.Count() })
                        .ToListAsync();

                    var conteosOrdenes = await _db.OrdenesGenerales
                        .Where(o => o.Activo
                                 && o.EstadoOrdenId < 3
                                 && o.FechaCreacion >= fechaConsulta
                                 && o.FechaCreacion < fechaSiguiente)
                        .SelectMany(o => o.Trabajos.Where(t => t.Activo && !t.RefaccionesListas),
                                    (o, t) => new { o.TipoOrdenId })
                        .GroupBy(x => x.TipoOrdenId)
                        .Select(g => new { TipoOrdenId = g.Key, Total = g.Count() })
                        .ToListAsync();

                    // Combinar sumando por TipoOrdenId
                    conteos = conteosCitas
                        .Concat(conteosOrdenes)
                        .GroupBy(x => x.TipoOrdenId)
                        .Select(g => (TipoOrdenId: g.Key, Total: g.Sum(x => x.Total)))
                        .ToList();
                }

                return Ok(new OrdenesActivasDto
                {
                    Success = true,
                    Message = "Conteo de refacciones pendientes obtenido exitosamente",
                    Servicios = conteos.FirstOrDefault(c => c.TipoOrdenId == 1).Total,
                    Diagnosticos = conteos.FirstOrDefault(c => c.TipoOrdenId == 2).Total,
                    Reparaciones = conteos.FirstOrDefault(c => c.TipoOrdenId == 3).Total,
                    Garantias = conteos.FirstOrDefault(c => c.TipoOrdenId == 4).Total,
                    Reacondicionamientos = conteos.FirstOrDefault(c => c.TipoOrdenId == 5).Total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conteo de refacciones pendientes");
                return StatusCode(500, new OrdenesActivasDto
                {
                    Success = false,
                    Message = "Error al obtener conteo de refacciones pendientes"
                });
            }
        }
    }
}