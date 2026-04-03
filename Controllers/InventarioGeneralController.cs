using CarSlineAPI.Data;
using CarSlineAPI.Models.DTOs;
using CarSlineAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarSlineAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventarioGeneralController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<InventarioGeneralController> _logger;

        public InventarioGeneralController(ApplicationDbContext db, ILogger<InventarioGeneralController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ============================================
        // VALIDAR NÚMERO DE PARTE
        // GET api/InventarioGeneral/validar/{numeroParte}
        // ============================================


        [HttpGet("validar/{numeroParte}")]
        [ProducesResponseType(typeof(ValidarNumeroParteResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidarNumeroParte(string numeroParte)
        {
            try
            {
                var refaccion = await _db.InventarioGeneral
                    .Where(r => r.NumeroParte == numeroParte.ToUpper())
                    .Select(r => new InventarioGeneralDto
                    {
                        Id = r.Id,
                        NumeroParte = r.NumeroParte,
                        TipoRefaccion = r.TipoRefaccion,
                        Ubicacion = r.Ubicacion,
                        Cantidad = r.Cantidad,
                        CantidadMinima = r.CantidadMinima,
                        UnidadMedida = r.UnidadMedida
                    })
                    .FirstOrDefaultAsync();

                if (refaccion != null)
                {
                    return Ok(new ValidarNumeroParteResponse
                    {
                        Existe = true,
                        Message = $"La refacción {numeroParte.ToUpper()} ya está registrada. Puedes registrar una entrada de stock.",
                        Refaccion = refaccion
                    });
                }

                return Ok(new ValidarNumeroParteResponse
                {
                    Existe = false,
                    Message = "Número de parte disponible. Puedes registrar la nueva refacción.",
                    Refaccion = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al validar número de parte: {numeroParte}");
                return StatusCode(500, new ValidarNumeroParteResponse
                {
                    Existe = false,
                    Message = "Error al validar número de parte"
                });
            }
        }

        // ============================================
        // CREAR REFACCIÓN (+ entrada + compatibilidad opcional)
        // POST api/InventarioGeneral/crear
        // ============================================
        [HttpPost("crear")]
        [ProducesResponseType(typeof(CrearRefaccionInventarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CrearRefaccion([FromBody] CrearRefaccionInventarioRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new CrearRefaccionInventarioResponse
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
                    // Verificar duplicado
                    var existe = await _db.InventarioGeneral
                        .AnyAsync(r => r.NumeroParte == request.NumeroParte.ToUpper());

                    if (existe)
                        return BadRequest(new CrearRefaccionInventarioResponse
                        {
                            Success = false,
                            Message = "Ya existe una refacción con ese número de parte. Usa el endpoint de entrada para agregar stock."
                        });

                    // Verificar almacenista
                    var almacenista = await _db.Usuarios
                        .AnyAsync(u => u.Id == request.AlmacenistaId && u.Activo);

                    if (!almacenista)
                        return BadRequest(new CrearRefaccionInventarioResponse
                        {
                            Success = false,
                            Message = "El almacenista no existe o no está activo"
                        });

                    // 1. Crear en InventarioGeneral
                    var refaccion = new InventarioGeneral
                    {
                        NumeroParte = request.NumeroParte.ToUpper(),
                        TipoRefaccion = request.TipoRefaccion,
                        Ubicacion = request.Ubicacion,
                        Cantidad = 0, // El trigger lo actualizará al insertar la entrada
                        CantidadMinima = request.CantidadMinima,
                        UnidadMedida = request.UnidadMedida
                    };

                    _db.InventarioGeneral.Add(refaccion);
                    await _db.SaveChangesAsync();

                    // 2. Registrar entrada inicial
                    var entrada = new EntradaInventario
                    {
                        InventarioId = refaccion.Id,
                        AlmacenistaId = request.AlmacenistaId,
                        Cantidad = request.CantidadInicial,
                        FechaEntrada = DateTime.Now
                    };

                    _db.EntradasInventario.Add(entrada);
                    await _db.SaveChangesAsync();

                    // 3. Registrar compatibilidad si viene en el request
                    int? compatibilidadId = null;

                    if (request.Compatibilidad != null)
                    {
                        var compat = new CompatibilidadRefaccion
                        {
                            InventarioId = refaccion.Id,
                            NumeroParte = refaccion.NumeroParte,   // ⭐ llenar desde la refacción
                            TipoRefaccion = refaccion.TipoRefaccion, // ⭐ llenar desde la refacción
                            Marca = request.Compatibilidad.Marca.ToUpper(),
                            Modelo = request.Compatibilidad.Modelo.ToUpper(),
                            Version = request.Compatibilidad.Version?.ToUpper(),
                            Motor = request.Compatibilidad.Motor,
                            AnioInicio = request.Compatibilidad.AnioInicio,
                            AnioFin = request.Compatibilidad.AnioFin,
                            Notas = request.Compatibilidad.Notas
                        };

                        _db.CompatibilidadRefacciones.Add(compat);
                        await _db.SaveChangesAsync();
                        compatibilidadId = compat.Id;
                    }

                    await transaction.CommitAsync();

                    _logger.LogInformation(
                        $"Refacción {refaccion.NumeroParte} creada con stock inicial {request.CantidadInicial}");

                    return Ok(new CrearRefaccionInventarioResponse
                    {
                        Success = true,
                        Message = "Refacción registrada exitosamente",
                        InventarioId = refaccion.Id,
                        EntradaId = entrada.Id,
                        CompatibilidadId = compatibilidadId
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al crear refacción en inventario");
                    return StatusCode(500, new CrearRefaccionInventarioResponse
                    {
                        Success = false,
                        Message = "Error al registrar la refacción"
                    });
                }
            });
        }

        // ============================================
        // REGISTRAR ENTRADA DE STOCK
        // POST api/InventarioGeneral/entrada
        // ============================================
        [HttpPost("entrada")]
        [ProducesResponseType(typeof(MovimientoInventarioResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> RegistrarEntrada([FromBody] RegistrarEntradaRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new MovimientoInventarioResponse
                {
                    Success = false,
                    Message = "Datos inválidos"
                });

            try
            {
                var refaccion = await _db.InventarioGeneral.FindAsync(request.InventarioId);

                if (refaccion == null)
                    return NotFound(new MovimientoInventarioResponse
                    {
                        Success = false,
                        Message = "Refacción no encontrada en inventario"
                    });

                var almacenista = await _db.Usuarios
                    .AnyAsync(u => u.Id == request.AlmacenistaId && u.Activo);

                if (!almacenista)
                    return BadRequest(new MovimientoInventarioResponse
                    {
                        Success = false,
                        Message = "Almacenista no válido"
                    });

                var entrada = new EntradaInventario
                {
                    InventarioId = request.InventarioId,
                    AlmacenistaId = request.AlmacenistaId,
                    Cantidad = request.Cantidad,
                    FechaEntrada = DateTime.Now
                };

                _db.EntradasInventario.Add(entrada);
                await _db.SaveChangesAsync();

                // Recargar para obtener stock actualizado por el trigger
                await _db.Entry(refaccion).ReloadAsync();

                _logger.LogInformation(
                    $"Entrada de {request.Cantidad} unidades registrada para {refaccion.NumeroParte}. Stock: {refaccion.Cantidad}");

                return Ok(new MovimientoInventarioResponse
                {
                    Success = true,
                    Message = $"Entrada registrada. Stock actual: {refaccion.Cantidad}",
                    MovimientoId = entrada.Id,
                    StockActual = refaccion.Cantidad
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al registrar entrada para inventario {request.InventarioId}");
                return StatusCode(500, new MovimientoInventarioResponse
                {
                    Success = false,
                    Message = "Error al registrar entrada"
                });
            }
        }

        // ============================================
        // REGISTRAR SALIDA DE STOCK
        // POST api/InventarioGeneral/salida
        // ============================================
        [HttpPost("salida")]
        [ProducesResponseType(typeof(MovimientoInventarioResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> RegistrarSalida([FromBody] RegistrarSalidaRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new MovimientoInventarioResponse
                {
                    Success = false,
                    Message = "Datos inválidos"
                });

            try
            {
                var refaccion = await _db.InventarioGeneral.FindAsync(request.InventarioId);

                if (refaccion == null)
                    return NotFound(new MovimientoInventarioResponse
                    {
                        Success = false,
                        Message = "Refacción no encontrada"
                    });

                // Validar stock antes de intentar (el trigger también lo valida, pero es mejor responder claro)
                if (refaccion.Cantidad < request.Cantidad)
                    return BadRequest(new MovimientoInventarioResponse
                    {
                        Success = false,
                        Message = $"Stock insuficiente. Disponible: {refaccion.Cantidad} {refaccion.UnidadMedida}(s)"
                    });

                var salida = new SalidaInventario
                {
                    InventarioId = request.InventarioId,
                    AlmacenistaId = request.AlmacenistaId,
                    TecnicoId = request.TecnicoId,
                    TrabajoId = request.TrabajoId,
                    Cantidad = request.Cantidad,
                    MotivoSalida = request.MotivoSalida,
                    FechaSalida = DateTime.Now
                };

                _db.SalidasInventario.Add(salida);
                await _db.SaveChangesAsync();

                await _db.Entry(refaccion).ReloadAsync();

                _logger.LogInformation(
                    $"Salida de {request.Cantidad} unidades registrada para {refaccion.NumeroParte}. Stock: {refaccion.Cantidad}");

                return Ok(new MovimientoInventarioResponse
                {
                    Success = true,
                    Message = $"Salida registrada. Stock actual: {refaccion.Cantidad}",
                    MovimientoId = salida.Id,
                    StockActual = refaccion.Cantidad
                });
            }
            catch (Exception ex)
            {
                // Atrapar el error del trigger de stock insuficiente
                if (ex.Message.Contains("Stock insuficiente") ||
                    (ex.InnerException?.Message.Contains("Stock insuficiente") ?? false))
                {
                    return BadRequest(new MovimientoInventarioResponse
                    {
                        Success = false,
                        Message = "Stock insuficiente para registrar la salida"
                    });
                }

                _logger.LogError(ex, $"Error al registrar salida para inventario {request.InventarioId}");
                return StatusCode(500, new MovimientoInventarioResponse
                {
                    Success = false,
                    Message = "Error al registrar salida"
                });
            }
        }

        // ============================================
        // BUSCADOR POR VEHÍCULO
        // GET api/InventarioGeneral/buscar-por-vehiculo
        // ============================================
        [HttpGet("buscar-por-vehiculo")]
        [ProducesResponseType(typeof(BuscarRefaccionVehiculoResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> BuscarPorVehiculo([FromQuery] BuscarRefaccionVehiculoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TipoRefaccion) ||
                string.IsNullOrWhiteSpace(request.Marca) ||
                string.IsNullOrWhiteSpace(request.Modelo))
            {
                return BadRequest(new BuscarRefaccionVehiculoResponse
                {
                    Success = false,
                    Message = "TipoRefaccion, Marca y Modelo son requeridos"
                });
            }

            try
            {
                var marca = request.Marca.ToUpper();
                var modelo = request.Modelo.ToUpper();
                var tipo = request.TipoRefaccion.ToUpper();
                var version = request.Version?.ToUpper();

                // Query base: buscar en compatibilidad por marca, modelo y tipo de refacción
                var query = _db.CompatibilidadRefacciones
                    .Include(c => c.Inventario)
                    .Where(c =>
                        c.Marca == marca &&
                        c.Modelo == modelo &&
                        c.Inventario!.TipoRefaccion.ToUpper().Contains(tipo));

                // Filtro de versión si viene
                if (!string.IsNullOrWhiteSpace(version))
                    query = query.Where(c => c.Version == null || c.Version == version);

                // Filtro de año si viene
                if (request.Anio.HasValue)
                    query = query.Where(c => c.AnioInicio <= request.Anio.Value && c.AnioFin >= request.Anio.Value);

                // Filtro de motor si viene
                if (!string.IsNullOrWhiteSpace(request.Motor))
                    query = query.Where(c => c.Motor == null || c.Motor == request.Motor);

                var compatibles = await query
                    .Select(c => new RefaccionDisponibleDto
                    {
                        InventarioId = c.Inventario!.Id,
                        NumeroParte = c.Inventario.NumeroParte,
                        TipoRefaccion = c.Inventario.TipoRefaccion,
                        Ubicacion = c.Inventario.Ubicacion,
                        StockDisponible = c.Inventario.Cantidad,
                        UnidadMedida = c.Inventario.UnidadMedida,
                        BajoStock = c.Inventario.Cantidad <= c.Inventario.CantidadMinima,
                        EsEquivalente = false,
                        Compatibilidad = $"{c.Marca} {c.Modelo}" +
                            (c.Version != null ? $" {c.Version}" : "") +
                            $" {c.AnioInicio}-{c.AnioFin}" +
                            (c.Motor != null ? $" {c.Motor}" : "")
                    })
                    .ToListAsync();

                // Buscar equivalentes de los resultados encontrados
                if (compatibles.Any())
                {
                    var idsEncontrados = compatibles.Select(c => c.InventarioId).ToList();

                    var equivalentes = await _db.RefaccionesEquivalentes
                        .Include(e => e.InventarioEquivalente)
                        .Where(e => idsEncontrados.Contains(e.InventarioId))
                        .Select(e => new RefaccionDisponibleDto
                        {
                            InventarioId = e.InventarioEquivalente!.Id,
                            NumeroParte = e.InventarioEquivalente.NumeroParte,
                            TipoRefaccion = e.InventarioEquivalente.TipoRefaccion,
                            Ubicacion = e.InventarioEquivalente.Ubicacion,
                            StockDisponible = e.InventarioEquivalente.Cantidad,
                            UnidadMedida = e.InventarioEquivalente.UnidadMedida,
                            BajoStock = e.InventarioEquivalente.Cantidad <= e.InventarioEquivalente.CantidadMinima,
                            EsEquivalente = true,
                            Compatibilidad = "Equivalente"
                        })
                        .ToListAsync();

                    // Agregar equivalentes que no estén ya en la lista
                    var idsYaIncluidos = compatibles.Select(c => c.InventarioId).ToHashSet();
                    var nuevosEquivalentes = equivalentes
                        .Where(e => !idsYaIncluidos.Contains(e.InventarioId))
                        .ToList();

                    compatibles.AddRange(nuevosEquivalentes);
                }

                bool busquedaAmpliada = false;

                // Si no encontró nada y había filtros opcionales, intentar búsqueda más amplia
                if (!compatibles.Any() && (request.Anio.HasValue || !string.IsNullOrWhiteSpace(request.Motor)))
                {
                    busquedaAmpliada = true;

                    var queryAmplia = _db.CompatibilidadRefacciones
                        .Include(c => c.Inventario)
                        .Where(c =>
                            c.Marca == marca &&
                            c.Modelo == modelo &&
                            c.Inventario!.TipoRefaccion.ToUpper().Contains(tipo));

                    compatibles = await queryAmplia
                        .Select(c => new RefaccionDisponibleDto
                        {
                            InventarioId = c.Inventario!.Id,
                            NumeroParte = c.Inventario.NumeroParte,
                            TipoRefaccion = c.Inventario.TipoRefaccion,
                            Ubicacion = c.Inventario.Ubicacion,
                            StockDisponible = c.Inventario.Cantidad,
                            UnidadMedida = c.Inventario.UnidadMedida,
                            BajoStock = c.Inventario.Cantidad <= c.Inventario.CantidadMinima,
                            EsEquivalente = false,
                            Compatibilidad = $"{c.Marca} {c.Modelo}" +
                                (c.Version != null ? $" {c.Version}" : "") +
                                $" {c.AnioInicio}-{c.AnioFin}"
                        })
                        .ToListAsync();
                }

                return Ok(new BuscarRefaccionVehiculoResponse
                {
                    Success = true,
                    Message = compatibles.Any()
                        ? $"Se encontraron {compatibles.Count} refacción(es)"
                        : "No se encontraron refacciones para ese vehículo",
                    Refacciones = compatibles,
                    TotalResultados = compatibles.Count,
                    BusquedaAmpliada = busquedaAmpliada
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de refacciones por vehículo");
                return StatusCode(500, new BuscarRefaccionVehiculoResponse
                {
                    Success = false,
                    Message = "Error al buscar refacciones"
                });
            }
        }

        // ============================================
        // LISTADO PAGINADO CON BÚSQUEDA
        // GET api/InventarioGeneral/paginado
        // ============================================
        [HttpGet("paginado")]
        [ProducesResponseType(typeof(InventarioPaginadoResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerPaginado(
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 10,
            [FromQuery] string? busqueda = null)
        {
            try
            {
                pagina = Math.Max(1, pagina);
                porPagina = Math.Clamp(porPagina, 5, 10);

                var query = _db.InventarioGeneral.AsQueryable();

                if (!string.IsNullOrWhiteSpace(busqueda))
                {
                    var b = busqueda.ToUpper();
                    query = query.Where(r =>
                        r.NumeroParte.ToUpper().Contains(b) ||
                        r.TipoRefaccion.ToUpper().Contains(b) ||
                        (r.Ubicacion != null && r.Ubicacion.ToUpper().Contains(b)));
                }

                var total = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(total / (double)porPagina);
                var totalBajoStock = await query.CountAsync(r => r.Cantidad <= r.CantidadMinima && r.Cantidad > 0);
                var totalSinStock = await query.CountAsync(r => r.Cantidad == 0);

                var refacciones = await query
                    .OrderBy(r => r.TipoRefaccion)
                    .ThenBy(r => r.NumeroParte)
                    .Skip((pagina - 1) * porPagina)
                    .Take(porPagina)
                    .Select(r => new InventarioGeneralDto
                    {
                        Id = r.Id,
                        NumeroParte = r.NumeroParte,
                        TipoRefaccion = r.TipoRefaccion,
                        Ubicacion = r.Ubicacion,
                        Cantidad = r.Cantidad,
                        CantidadMinima = r.CantidadMinima,
                        UnidadMedida = r.UnidadMedida
                    })
                    .ToListAsync();

                return Ok(new InventarioPaginadoResponse
                {
                    Success = true,
                    Refacciones = refacciones,
                    PaginaActual = pagina,
                    TotalPaginas = totalPages,
                    TotalItems = total,
                    PorPagina = porPagina,
                    TienePaginaAnterior = pagina > 1,
                    TienePaginaSiguiente = pagina < totalPages,
                    TotalBajoStock = totalBajoStock,
                    TotalSinStock = totalSinStock
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inventario paginado");
                return StatusCode(500, new { Message = "Error al obtener inventario" });
            }
        }

        // ============================================
        // DETALLE DE REFACCIÓN (con compatibilidades, equivalencias y movimientos)
        // GET api/InventarioGeneral/{id}/detalle
        // ============================================
        [HttpGet("{id}/detalle")]
        [ProducesResponseType(typeof(DetalleRefaccionResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerDetalle(int id)
        {
            try
            {
                var refaccion = await _db.InventarioGeneral.FindAsync(id);

                if (refaccion == null)
                    return NotFound(new DetalleRefaccionResponse
                    {
                        Success = false,
                        Message = "Refacción no encontrada"
                    });

                var compatibilidades = await _db.CompatibilidadRefacciones
                    .Where(c => c.InventarioId == id)
                    .Select(c => new CompatibilidadDto
                    {
                        Id = c.Id,
                        Marca = c.Marca,
                        Modelo = c.Modelo,
                        Version = c.Version,
                        Motor = c.Motor,
                        AnioInicio = c.AnioInicio,
                        AnioFin = c.AnioFin,
                        Notas = c.Notas
                    })
                    .ToListAsync();

                var equivalencias = await _db.RefaccionesEquivalentes
                    .Include(e => e.InventarioEquivalente)
                    .Where(e => e.InventarioId == id)
                    .Select(e => new EquivalenciaDto
                    {
                        Id = e.Id,
                        InventarioEquivalenteId = e.InventarioEquivalenteId,
                        NumeroParteEquivalente = e.InventarioEquivalente!.NumeroParte,
                        TipoRefaccion = e.InventarioEquivalente.TipoRefaccion,
                        StockEquivalente = e.InventarioEquivalente.Cantidad
                    })
                    .ToListAsync();

                // Últimos 10 movimientos (entradas + salidas mezclados y ordenados)
                var entradas = await _db.EntradasInventario
                    .Include(e => e.Almacenista)
                    .Where(e => e.InventarioId == id)
                    .OrderByDescending(e => e.FechaEntrada)
                    .Take(10)
                    .Select(e => new MovimientoDto
                    {
                        Tipo = "Entrada",
                        Cantidad = e.Cantidad,
                        Fecha = e.FechaEntrada,
                        Almacenista = e.Almacenista!.NombreCompleto,
                        Motivo = null
                    })
                    .ToListAsync();

                var salidas = await _db.SalidasInventario
                    .Include(s => s.Almacenista)
                    .Where(s => s.InventarioId == id)
                    .OrderByDescending(s => s.FechaSalida)
                    .Take(10)
                    .Select(s => new MovimientoDto
                    {
                        Tipo = "Salida",
                        Cantidad = s.Cantidad,
                        Fecha = s.FechaSalida,
                        Almacenista = s.Almacenista!.NombreCompleto,
                        Motivo = s.MotivoSalida
                    })
                    .ToListAsync();

                var movimientos = entradas
                    .Concat(salidas)
                    .OrderByDescending(m => m.Fecha)
                    .Take(10)
                    .ToList();

                return Ok(new DetalleRefaccionResponse
                {
                    Success = true,
                    Message = "Detalle obtenido exitosamente",
                    Refaccion = new InventarioGeneralDto
                    {
                        Id = refaccion.Id,
                        NumeroParte = refaccion.NumeroParte,
                        TipoRefaccion = refaccion.TipoRefaccion,
                        Ubicacion = refaccion.Ubicacion,
                        Cantidad = refaccion.Cantidad,
                        CantidadMinima = refaccion.CantidadMinima,
                        UnidadMedida = refaccion.UnidadMedida
                    },
                    Compatibilidades = compatibilidades,
                    Equivalencias = equivalencias,
                    UltimosMovimientos = movimientos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener detalle de refacción {id}");
                return StatusCode(500, new DetalleRefaccionResponse
                {
                    Success = false,
                    Message = "Error al obtener detalle"
                });
            }
        }

        // ============================================
        // AGREGAR COMPATIBILIDAD A REFACCIÓN EXISTENTE
        // POST api/InventarioGeneral/compatibilidad
        // ============================================
        [HttpPost("compatibilidad")]
        public async Task<IActionResult> AgregarCompatibilidad([FromBody] AgregarCompatibilidadRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new GenericInventarioResponse { Success = false, Message = "Datos inválidos" });

            try
            {
                // ⭐ Necesitamos traer la refacción para obtener NumeroParte y TipoRefaccion
                var refaccion = await _db.InventarioGeneral.FindAsync(request.InventarioId);

                if (refaccion == null)
                    return NotFound(new GenericInventarioResponse
                    {
                        Success = false,
                        Message = "Refacción no encontrada"
                    });

                var compat = new CompatibilidadRefaccion
                {
                    InventarioId = request.InventarioId,
                    NumeroParte = refaccion.NumeroParte,   // ⭐
                    TipoRefaccion = refaccion.TipoRefaccion, // ⭐
                    Marca = request.Compatibilidad.Marca.ToUpper(),
                    Modelo = request.Compatibilidad.Modelo.ToUpper(),
                    Version = request.Compatibilidad.Version?.ToUpper(),
                    Motor = request.Compatibilidad.Motor,
                    AnioInicio = request.Compatibilidad.AnioInicio,
                    AnioFin = request.Compatibilidad.AnioFin,
                    Notas = request.Compatibilidad.Notas
                };

                _db.CompatibilidadRefacciones.Add(compat);
                await _db.SaveChangesAsync();

                return Ok(new GenericInventarioResponse
                {
                    Success = true,
                    Message = "Compatibilidad registrada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al agregar compatibilidad a inventario {request.InventarioId}");
                return StatusCode(500, new GenericInventarioResponse
                {
                    Success = false,
                    Message = "Error al agregar compatibilidad"
                });
            }
        }
        // ============================================
        // AGREGAR EQUIVALENCIA ENTRE REFACCIONES
        // POST api/InventarioGeneral/equivalencia
        // ============================================
        [HttpPost("equivalencia")]
        public async Task<IActionResult> AgregarEquivalencia([FromBody] AgregarEquivalenciaRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new GenericInventarioResponse { Success = false, Message = "Datos inválidos" });

            try
            {
                if (request.InventarioId == request.InventarioEquivalenteId)
                    return BadRequest(new GenericInventarioResponse
                    {
                        Success = false,
                        Message = "Una refacción no puede ser equivalente de sí misma"
                    });

                // ⭐ Traer ambas refacciones para obtener NumeroParte y TipoRefaccion
                var refaccion = await _db.InventarioGeneral.FindAsync(request.InventarioId);
                var equivalente = await _db.InventarioGeneral.FindAsync(request.InventarioEquivalenteId);

                if (refaccion == null || equivalente == null)
                    return NotFound(new GenericInventarioResponse
                    {
                        Success = false,
                        Message = "Una o ambas refacciones no fueron encontradas"
                    });

                var yaExiste = await _db.RefaccionesEquivalentes
                    .AnyAsync(e =>
                        (e.InventarioId == request.InventarioId &&
                         e.InventarioEquivalenteId == request.InventarioEquivalenteId) ||
                        (e.InventarioId == request.InventarioEquivalenteId &&
                         e.InventarioEquivalenteId == request.InventarioId));

                if (yaExiste)
                    return BadRequest(new GenericInventarioResponse
                    {
                        Success = false,
                        Message = "Esta equivalencia ya está registrada"
                    });

                var equivalencia = new RefaccionEquivalente
                {
                    InventarioId = request.InventarioId,
                    NumeroParte = refaccion.NumeroParte,    // ⭐
                    TipoRefaccion = refaccion.TipoRefaccion,  // ⭐
                    InventarioEquivalenteId = request.InventarioEquivalenteId
                    // TipoEquivalencia eliminado
                };

                _db.RefaccionesEquivalentes.Add(equivalencia);
                await _db.SaveChangesAsync();

                return Ok(new GenericInventarioResponse
                {
                    Success = true,
                    Message = "Equivalencia registrada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar equivalencia");
                return StatusCode(500, new GenericInventarioResponse
                {
                    Success = false,
                    Message = "Error al agregar equivalencia"
                });
            }
        }
        // ============================================
        // ALERTAS DE BAJO STOCK Y SIN STOCK
        // GET api/InventarioGeneral/alertas
        // ============================================
        [HttpGet("alertas")]
        [ProducesResponseType(typeof(AlertasInventarioResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerAlertas()
        {
            try
            {
                var bajoStock = await _db.InventarioGeneral
                    .Where(r => r.Cantidad <= r.CantidadMinima && r.Cantidad > 0)
                    .Select(r => new InventarioGeneralDto
                    {
                        Id = r.Id,
                        NumeroParte = r.NumeroParte,
                        TipoRefaccion = r.TipoRefaccion,
                        Ubicacion = r.Ubicacion,
                        Cantidad = r.Cantidad,
                        CantidadMinima = r.CantidadMinima,
                        UnidadMedida = r.UnidadMedida
                    })
                    .OrderBy(r => r.Cantidad)
                    .ToListAsync();

                var sinStock = await _db.InventarioGeneral
                    .Where(r => r.Cantidad == 0)
                    .Select(r => new InventarioGeneralDto
                    {
                        Id = r.Id,
                        NumeroParte = r.NumeroParte,
                        TipoRefaccion = r.TipoRefaccion,
                        Ubicacion = r.Ubicacion,
                        Cantidad = r.Cantidad,
                        CantidadMinima = r.CantidadMinima,
                        UnidadMedida = r.UnidadMedida
                    })
                    .OrderBy(r => r.TipoRefaccion)
                    .ToListAsync();

                return Ok(new AlertasInventarioResponse
                {
                    Success = true,
                    TotalBajoStock = bajoStock.Count,
                    TotalSinStock = sinStock.Count,
                    BajoStock = bajoStock,
                    SinStock = sinStock
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener alertas de inventario");
                return StatusCode(500, new AlertasInventarioResponse { Success = false });
            }
        }

        // ============================================
        // ELIMINAR COMPATIBILIDAD
        // DELETE api/InventarioGeneral/compatibilidad/{id}
        // ============================================
        [HttpDelete("compatibilidad/{id}")]
        [ProducesResponseType(typeof(GenericInventarioResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> EliminarCompatibilidad(int id)
        {
            try
            {
                var compat = await _db.CompatibilidadRefacciones.FindAsync(id);

                if (compat == null)
                    return NotFound(new GenericInventarioResponse
                    {
                        Success = false,
                        Message = "Compatibilidad no encontrada"
                    });

                _db.CompatibilidadRefacciones.Remove(compat);
                await _db.SaveChangesAsync();

                return Ok(new GenericInventarioResponse
                {
                    Success = true,
                    Message = "Compatibilidad eliminada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar compatibilidad {id}");
                return StatusCode(500, new GenericInventarioResponse
                {
                    Success = false,
                    Message = "Error al eliminar compatibilidad"
                });
            }
        }
    }
}