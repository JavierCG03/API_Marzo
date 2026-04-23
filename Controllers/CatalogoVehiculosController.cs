using CarSlineAPI.Data;
using CarSlineAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CarSlineAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogoVehiculosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CatalogoVehiculosController> _logger;

        public CatalogoVehiculosController(ApplicationDbContext db, ILogger<CatalogoVehiculosController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ══════════════════════════════════════════════════
        // CONSULTAS — réplica exacta de los métodos locales
        // ══════════════════════════════════════════════════

        /// <summary>
        /// Lista todas las marcas activas ordenadas alfabéticamente.
        /// Replica: ObtenerMarcas()
        /// GET api/CatalogoVehiculos/marcas
        /// </summary>
        [HttpGet("marcas")]
        public async Task<IActionResult> ObtenerMarcas()
        {
            var marcas = await _db.Marcas
                .Where(m => m.Activo)
                .OrderBy(m => m.Nombre)
                .Select(m => new { m.Id, m.Nombre })
                .ToListAsync();

            return Ok(marcas);
        }

        /// <summary>
        /// Lista los modelos de una marca por su ID.
        /// Replica: ObtenerModelos(string marca)
        /// GET api/CatalogoVehiculos/modelos/{marcaId}
        /// </summary>
        [HttpGet("modelos/{marcaId}")]
        public async Task<IActionResult> ObtenerModelos(int marcaId)
        {
            var marcaExiste = await _db.Marcas.AnyAsync(m => m.Id == marcaId && m.Activo);
            if (!marcaExiste)
                return NotFound(new { Success = false, Message = "Marca no encontrada" });

            var modelos = await _db.Modelos
                .Where(m => m.MarcaId == marcaId && m.Activo)
                .OrderBy(m => m.Nombre)
                .Select(m => new { m.Id, m.Nombre, m.TipoCarroceria })
                .ToListAsync();

            return Ok(modelos);
        }

        /// <summary>
        /// Lista los modelos de una marca filtrados por tipo de carrocería.
        /// Replica: ObtenerModelosPorTipo(string marca, TipoCarroceria tipo)
        /// GET api/CatalogoVehiculos/modelos/{marcaId}/tipo/{tipoCarroceria}
        /// </summary>
        [HttpGet("modelos/{marcaId}/tipo/{tipoCarroceria}")]
        public async Task<IActionResult> ObtenerModelosPorTipo(int marcaId, string tipoCarroceria)
        {
            var marcaExiste = await _db.Marcas.AnyAsync(m => m.Id == marcaId && m.Activo);
            if (!marcaExiste)
                return NotFound(new { Success = false, Message = "Marca no encontrada" });

            var modelos = await _db.Modelos
                .Where(m => m.MarcaId == marcaId
                         && m.Activo
                         && m.TipoCarroceria.ToLower() == tipoCarroceria.ToLower())
                .OrderBy(m => m.Nombre)
                .Select(m => new { m.Id, m.Nombre, m.TipoCarroceria })
                .ToListAsync();

            return Ok(modelos);
        }

        /// <summary>
        /// Lista las versiones de un modelo por su ID.
        /// Replica: ObtenerVersiones(string marca, string modelo)
        /// GET api/CatalogoVehiculos/versiones/{modeloId}
        /// </summary>
        [HttpGet("versiones/{modeloId}")]
        public async Task<IActionResult> ObtenerVersiones(int modeloId)
        {
            var modeloExiste = await _db.Modelos.AnyAsync(m => m.Id == modeloId && m.Activo);
            if (!modeloExiste)
                return NotFound(new { Success = false, Message = "Modelo no encontrado" });

            var versiones = await _db.Versiones
                .Where(v => v.ModeloId == modeloId && v.Activo)
                .OrderBy(v => v.Nombre)
                .Select(v => new { v.Id, v.Nombre })
                .ToListAsync();

            return Ok(versiones);
        }

        /// <summary>
        /// Devuelve el tipo de carrocería de un modelo.
        /// Replica: ObtenerTipo(string marca, string modelo)
        /// GET api/CatalogoVehiculos/tipo/{modeloId}
        /// </summary>
        [HttpGet("tipo/{modeloId}")]
        public async Task<IActionResult> ObtenerTipo(int modeloId)
        {
            var modelo = await _db.Modelos
                .Where(m => m.Id == modeloId && m.Activo)
                .Select(m => new { m.Id, m.Nombre, m.TipoCarroceria })
                .FirstOrDefaultAsync();

            if (modelo == null)
                return NotFound(new { Success = false, Message = "Modelo no encontrado" });

            return Ok(new { Success = true, modelo.Id, modelo.Nombre, modelo.TipoCarroceria });
        }

        /// <summary>
        /// Verifica si una marca existe. Replica: ExisteMarca(string marca)
        /// GET api/CatalogoVehiculos/existe-marca/{marcaId}
        /// </summary>
        [HttpGet("existe-marca/{marcaId}")]
        public async Task<IActionResult> ExisteMarca(int marcaId)
        {
            var existe = await _db.Marcas.AnyAsync(m => m.Id == marcaId && m.Activo);
            return Ok(new { Existe = existe });
        }

        /// <summary>
        /// Verifica si un modelo existe para una marca.
        /// Replica: ExisteModelo(string marca, string modelo)
        /// GET api/CatalogoVehiculos/existe-modelo/{marcaId}/{modeloId}
        /// </summary>
        [HttpGet("existe-modelo/{marcaId}/{modeloId}")]
        public async Task<IActionResult> ExisteModelo(int marcaId, int modeloId)
        {
            var existe = await _db.Modelos
                .AnyAsync(m => m.Id == modeloId && m.MarcaId == marcaId && m.Activo);
            return Ok(new { Existe = existe });
        }

        // ══════════════════════════════════════════════════
        // ADMINISTRACIÓN — CRUD para gestionar el catálogo
        // ══════════════════════════════════════════════════

        /// <summary>
        /// Crear nueva marca.
        /// POST api/CatalogoVehiculos/marcas
        /// </summary>
        [HttpPost("marcas")]
        public async Task<IActionResult> CrearMarca([FromBody] CrearMarcaRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Datos inválidos" });

            var existe = await _db.Marcas.AnyAsync(m => m.Nombre.ToLower() == request.Nombre.ToLower());
            if (existe)
                return BadRequest(new { Success = false, Message = $"La marca '{request.Nombre}' ya existe" });

            var marca = new Marca { Nombre = request.Nombre.Trim(), Activo = true };
            _db.Marcas.Add(marca);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Marca creada: {Nombre} (ID {Id})", marca.Nombre, marca.Id);
            return Ok(new { Success = true, Message = "Marca creada exitosamente", MarcaId = marca.Id });
        }

        /// <summary>
        /// Crear nuevo modelo para una marca.
        /// POST api/CatalogoVehiculos/modelos
        /// </summary>
        [HttpPost("modelos")]
        public async Task<IActionResult> CrearModelo([FromBody] CrearModeloRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Datos inválidos" });

            var marcaExiste = await _db.Marcas.AnyAsync(m => m.Id == request.MarcaId && m.Activo);
            if (!marcaExiste)
                return NotFound(new { Success = false, Message = "Marca no encontrada" });

            var existe = await _db.Modelos.AnyAsync(m =>
                m.MarcaId == request.MarcaId &&
                m.Nombre.ToLower() == request.Nombre.ToLower());
            if (existe)
                return BadRequest(new { Success = false, Message = $"El modelo '{request.Nombre}' ya existe para esta marca" });

            var modelo = new Modelo
            {
                MarcaId = request.MarcaId,
                Nombre = request.Nombre.Trim(),
                TipoCarroceria = request.TipoCarroceria.Trim(),
                Activo = true
            };

            _db.Modelos.Add(modelo);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Modelo creado: {Nombre} (ID {Id})", modelo.Nombre, modelo.Id);
            return Ok(new { Success = true, Message = "Modelo creado exitosamente", ModeloId = modelo.Id });
        }

        /// <summary>
        /// Crear nueva versión para un modelo.
        /// POST api/CatalogoVehiculos/versiones
        /// </summary>
        [HttpPost("versiones")]
        public async Task<IActionResult> CrearVersion([FromBody] CrearVersionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Datos inválidos" });

            var modeloExiste = await _db.Modelos.AnyAsync(m => m.Id == request.ModeloId && m.Activo);
            if (!modeloExiste)
                return NotFound(new { Success = false, Message = "Modelo no encontrado" });

            var existe = await _db.Versiones.AnyAsync(v =>
                v.ModeloId == request.ModeloId &&
                v.Nombre.ToLower() == request.Nombre.ToLower());
            if (existe)
                return BadRequest(new { Success = false, Message = $"La versión '{request.Nombre}' ya existe para este modelo" });

            var version = new Versiona
            {
                ModeloId = request.ModeloId,
                Nombre = request.Nombre.Trim(),
                Activo = true
            };

            _db.Versiones.Add(version);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Versión creada: {Nombre} (ID {Id})", version.Nombre, version.Id);
            return Ok(new { Success = true, Message = "Versión creada exitosamente", VersionId = version.Id });
        }

        /// <summary>
        /// Activar o desactivar una marca (borrado lógico).
        /// PUT api/CatalogoVehiculos/marcas/{id}/estado
        /// </summary>
        [HttpPut("marcas/{id}/estado")]
        public async Task<IActionResult> CambiarEstadoMarca(int id, [FromBody] CambiarEstadoRequest request)
        {
            var marca = await _db.Marcas.FindAsync(id);
            if (marca == null)
                return NotFound(new { Success = false, Message = "Marca no encontrada" });

            marca.Activo = request.Activo;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Message = request.Activo ? "Marca activada" : "Marca desactivada",
                MarcaId = id
            });
        }

        /// <summary>
        /// Activar o desactivar un modelo.
        /// PUT api/CatalogoVehiculos/modelos/{id}/estado
        /// </summary>
        [HttpPut("modelos/{id}/estado")]
        public async Task<IActionResult> CambiarEstadoModelo(int id, [FromBody] CambiarEstadoRequest request)
        {
            var modelo = await _db.Modelos.FindAsync(id);
            if (modelo == null)
                return NotFound(new { Success = false, Message = "Modelo no encontrado" });

            modelo.Activo = request.Activo;
            await _db.SaveChangesAsync();

            return Ok(new { Success = true, Message = request.Activo ? "Modelo activado" : "Modelo desactivado" });
        }

        /// <summary>
        /// Activar o desactivar una versión.
        /// PUT api/CatalogoVehiculos/versiones/{id}/estado
        /// </summary>
        [HttpPut("versiones/{id}/estado")]
        public async Task<IActionResult> CambiarEstadoVersion(int id, [FromBody] CambiarEstadoRequest request)
        {
            var version = await _db.Versiones.FindAsync(id);
            if (version == null)
                return NotFound(new { Success = false, Message = "Versión no encontrada" });

            version.Activo = request.Activo;
            await _db.SaveChangesAsync();

            return Ok(new { Success = true, Message = request.Activo ? "Versión activada" : "Versión desactivada" });
        }

        /// <summary>
        /// Cascada completa: dado un marcaId devuelve modelos y sus versiones de una sola llamada.
        /// Útil para cargar el formulario de vehículo completo de golpe.
        /// GET api/CatalogoVehiculos/cascada/{marcaId}
        /// </summary>
        [HttpGet("cascada/{marcaId}")]
        public async Task<IActionResult> ObtenerCascada(int marcaId)
        {
            var marca = await _db.Marcas
                .Where(m => m.Id == marcaId && m.Activo)
                .FirstOrDefaultAsync();

            if (marca == null)
                return NotFound(new { Success = false, Message = "Marca no encontrada" });

            var modelos = await _db.Modelos
                .Where(m => m.MarcaId == marcaId && m.Activo)
                .OrderBy(m => m.Nombre)
                .Select(m => new
                {
                    m.Id,
                    m.Nombre,
                    m.TipoCarroceria,
                    Versiones = _db.Versiones
                        .Where(v => v.ModeloId == m.Id && v.Activo)
                        .OrderBy(v => v.Nombre)
                        .Select(v => new { v.Id, v.Nombre })
                        .ToList()
                })
                .ToListAsync();

            return Ok(new
            {
                Success = true,
                MarcaId = marcaId,
                MarcaNombre = marca.Nombre,
                Modelos = modelos
            });
        }
    }

    // ══════════════════════════════════════════════════
    // REQUEST DTOs
    // ══════════════════════════════════════════════════

    public class CrearMarcaRequest
    {
        [Required(ErrorMessage = "El nombre de la marca es requerido")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
    }

    public class CrearModeloRequest
    {
        [Required]
        public int MarcaId { get; set; }

        [Required(ErrorMessage = "El nombre del modelo es requerido")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de carrocería es requerido")]
        [MaxLength(20)]
        public string TipoCarroceria { get; set; } = string.Empty;
    }

    public class CrearVersionRequest
    {
        [Required]
        public int ModeloId { get; set; }

        [Required(ErrorMessage = "El nombre de la versión es requerido")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
    }

    public class CambiarEstadoRequest
    {
        public bool Activo { get; set; }
    }
}