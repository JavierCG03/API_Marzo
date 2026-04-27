using CarSlineAPI.Data;
using CarSlineAPI.Models.DTOs;
using CarSlineAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarSlineAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckListAvaluoController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CheckListAvaluoController> _logger;

        public CheckListAvaluoController(ApplicationDbContext db, ILogger<CheckListAvaluoController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Guardar o actualizar checklist de avalúo
        /// POST api/CheckListAvaluo/guardar
        /// </summary>
        [HttpPost("guardar")]
        [ProducesResponseType(typeof(CheckListAvaluoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GuardarCheckList([FromBody] GuardarCheckListAvaluoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new CheckListAvaluoResponse
                {
                    Success = false,
                    Message = "Datos inválidos: " + string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });

            try
            {
                // Verificar que el avalúo existe
                var avaluo = await _db.DatosAvaluos
                    .FirstOrDefaultAsync(a => a.Id == request.AvaluoId && a.Activo);

                if (avaluo == null)
                    return NotFound(new CheckListAvaluoResponse
                    {
                        Success = false,
                        Message = "Avalúo no encontrado o inactivo"
                    });

                // Verificar que el vigilante existe
                var vigilante = await _db.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == request.VigilanteId && u.Activo);

                if (vigilante == null)
                    return NotFound(new CheckListAvaluoResponse
                    {
                        Success = false,
                        Message = "Vigilante no encontrado o inactivo"
                    });

                // Buscar si ya existe checklist para este avalúo
                var existente = await _db.CheckListAvaluos
                    .FirstOrDefaultAsync(c => c.AvaluoId == request.AvaluoId);

                if (existente != null)
                {
                    // Actualizar
                    MapearDesdeRequest(existente, request);
                    _logger.LogInformation("CheckList avalúo {AvaluoId} actualizado", request.AvaluoId);
                }
                else
                {
                    // Crear nuevo
                    var nuevo = new CheckListAvaluo { AvaluoId = request.AvaluoId };
                    MapearDesdeRequest(nuevo, request);
                    _db.CheckListAvaluos.Add(nuevo);
                    _logger.LogInformation("CheckList avalúo {AvaluoId} creado", request.AvaluoId);
                    avaluo.CheckList = true;
                }

                await _db.SaveChangesAsync();

                return Ok(new CheckListAvaluoResponse
                {
                    Success = true,
                    Message = existente != null
                        ? "CheckList actualizado exitosamente"
                        : "CheckList creado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar CheckList del avalúo {AvaluoId}", request.AvaluoId);
                return StatusCode(500, new CheckListAvaluoResponse
                {
                    Success = false,
                    Message = "Error al guardar el checklist"
                });
            }
        }

        /// <summary>
        /// Obtener checklist por AvaluoId
        /// GET api/CheckListAvaluo/avaluo/{avaluoId}
        /// </summary>
        [HttpGet("avaluo/{avaluoId}")]
        [ProducesResponseType(typeof(CheckListAvaluoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorAvaluo(int avaluoId)
        {
            try
            {
                var checkList = await _db.CheckListAvaluos
                    .Include(c => c.Vigilante)
                    .FirstOrDefaultAsync(c => c.AvaluoId == avaluoId);

                if (checkList == null)
                    return NotFound(new CheckListAvaluoResponse
                    {
                        Success = false,
                        Message = "No existe checklist para este avalúo"
                    });

                return Ok(new CheckListAvaluoResponse
                {
                    Success = true,
                    Message = "CheckList encontrado",
                    CheckList = MapearADto(checkList)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener CheckList del avalúo {AvaluoId}", avaluoId);
                return StatusCode(500, new CheckListAvaluoResponse
                {
                    Success = false,
                    Message = "Error al obtener el checklist"
                });
            }
        }

        // ============================================
        // MÉTODOS PRIVADOS
        // ============================================

        private static void MapearDesdeRequest(CheckListAvaluo entity, GuardarCheckListAvaluoRequest r)
        {
            entity.VigilanteId = r.VigilanteId;
            entity.Kilometraje = r.Kilometraje;
            entity.Combustible = r.Combustible;
            entity.DuplicadoLlave = r.DuplicadoLlave;
            entity.BirloSeguridad = r.BirloSeguridad;
            entity.CandadoSeguridad = r.CandadoSeguridad;
            entity.TapaCajuela = r.TapaCajuela;
            entity.CortinaCajuela = r.CortinaCajuela;
            entity.Refaccion = r.Refaccion;
            entity.Gato = r.Gato;
            entity.Maneral = r.Maneral;
            entity.LlaveLoCruz = r.LlaveLoCruz;
            entity.GanchoArrastre = r.GanchoArrastre;
            entity.TaponGasolina = r.TaponGasolina;
            entity.Rines = r.Rines;
            entity.Tapones = r.Tapones;
            entity.CentroRin = r.CentroRin;
            entity.RadioEstereo = r.RadioEstereo;
            entity.LimpiadorDelantero = r.LimpiadorDelantero;
            entity.LimpiadorTrasero = r.LimpiadorTrasero;
            entity.Viseras = r.Viseras;
            entity.Cabeceras = r.Cabeceras;
            entity.Tapetes = r.Tapetes;
            entity.FarosNiebla = r.FarosNiebla;
            entity.Bateria = r.Bateria;
            entity.VarillasPickUp = r.VarillasPickUp;
            entity.Encendedor = r.Encendedor;
            entity.AntenaRadio = r.AntenaRadio;
            entity.Maletin = r.Maletin;
            entity.Cubresala = r.Cubresala;
            entity.Cubrevolante = r.Cubrevolante;

            entity.MarcaBateria = r.MarcaBateria;
            entity.MarcaLlantaDelanteraDer = r.MarcaLlantaDelanteraDer;
            entity.MarcaLlantaDelanteraIzq = r.MarcaLlantaDelanteraIzq;
            entity.MarcaLlantaTraseraDer = r.MarcaLlantaTraseraDer;
            entity.MarcaLlantaTraseraIzq = r.MarcaLlantaTraseraIzq;
            entity.MedidaLlantaDelanteraDer = r.MedidaLlantaDelanteraDer;
            entity.MedidaLlantaDelanteraIzq = r.MedidaLlantaDelanteraIzq;
            entity.MedidaLlantaTraseraDer = r.MedidaLlantaTraseraDer;
            entity.MedidaLlantaTraseraIzq = r.MedidaLlantaTraseraIzq;
            entity.MarcaLlantaRefaccion = r.MarcaLlantaRefaccion;
            entity.MedidaLlantaRefaccion = r.MedidaLlantaRefaccion;

            entity.Comentarios = r.Comentarios;
            entity.Observaciones = r.Observaciones;
        }

        private static CheckListAvaluoDto MapearADto(CheckListAvaluo c) => new()
        {
            Id = c.Id,
            AvaluoId = c.AvaluoId,
            VigilanteId = c.VigilanteId,
            VigilanteNombre = c.Vigilante?.NombreCompleto ?? "",
            Kilometraje = c.Kilometraje,
            Combustible = c.Combustible,
            DuplicadoLlave = c.DuplicadoLlave,
            BirloSeguridad = c.BirloSeguridad,
            CandadoSeguridad = c.CandadoSeguridad,
            TapaCajuela = c.TapaCajuela,
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
            AntenaRadio = c.AntenaRadio,
            Maletin = c.Maletin,
            Cubresala = c.Cubresala,
            Cubrevolante = c.Cubrevolante,
            MarcaBateria = c.MarcaBateria,
            MarcaLlantaDelanteraDer = c.MarcaLlantaDelanteraDer,
            MarcaLlantaDelanteraIzq = c.MarcaLlantaDelanteraIzq,
            MarcaLlantaTraseraDer = c.MarcaLlantaTraseraDer,
            MarcaLlantaTraseraIzq = c.MarcaLlantaTraseraIzq,
            MedidaLlantaDelanteraDer = c.MedidaLlantaDelanteraDer,
            MedidaLlantaDelanteraIzq = c.MedidaLlantaDelanteraIzq,
            MedidaLlantaTraseraDer = c.MedidaLlantaTraseraDer,
            MedidaLlantaTraseraIzq = c.MedidaLlantaTraseraIzq,
            MarcaLlantaRefaccion = c.MarcaLlantaRefaccion,
            MedidaLlantaRefaccion = c.MedidaLlantaRefaccion,
            Comentarios = c.Comentarios,
            Observaciones = c.Observaciones
        };
    }
}