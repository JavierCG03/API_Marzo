using System.ComponentModel.DataAnnotations;

namespace CarSlineAPI.Models.DTOs
{
    // ============================================
    // REQUEST DTOs — REACONDICIONAMIENTO ESTÉTICO
    // ============================================

    public class CrearReacondicionamientoEsteticoRequest
    {
        [Required]
        public int EncargadoEsteticaId { get; set; }

        [Required]
        public int VehiculoId { get; set; }

        [Required, MinLength(1, ErrorMessage = "Debe agregar al menos un trabajo")]
        public List<TrabajoEsteticoRequest> Trabajos { get; set; } = new();
    }

    public class TrabajoEsteticoRequest
    {
        [Required, MaxLength(255)]
        public string Trabajo { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? EmpresaQueRealizara { get; set; }

        public string? IndicacionesTrabajo { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CostoTrabajo { get; set; } = 0.00m;
    }

    public class AgregarTrabajoEsteticoRequest
    {
        [Required]
        public int ReacondicionamientoEsteticoId { get; set; }

        [Required, MaxLength(255)]
        public string Trabajo { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? EmpresaQueRealizara { get; set; }

        public string? IndicacionesTrabajo { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CostoTrabajo { get; set; } = 0.00m;
    }

    public class ActualizarTrabajoEsteticoRequest
    {
        [MaxLength(255)]
        public string? EmpresaQueRealizara { get; set; }

        public string? IndicacionesTrabajo { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? CostoTrabajo { get; set; }
    }

    public class CambiarEstadoTrabajoEsteticoRequest
    {
        /// <summary>1 = Pendiente, 2 = En Proceso, 3 = Completado</summary>
        [Required]
        [Range(1, 3, ErrorMessage = "Estado debe ser 1 (Pendiente), 2 (En Proceso) o 3 (Completado)")]
        public int NuevoEstado { get; set; }
    }

    // ============================================
    // REQUEST DTOs — REACONDICIONAMIENTO GENERAL
    // ============================================

    public class CrearReacondicionamientoVehiculoRequest
    {
        [Required]
        public int AvaluoId { get; set; }

        [Required]
        public int VehiculoId { get; set; }

        [Required]
        public int ReacondicionamientoMecanicoId { get; set; }

        [Required]
        public int ReacondicionamientoEsteticoId { get; set; }

        public bool TieneReacondicionamientoMecanico { get; set; } = false;
        public bool TieneReacondicionamientoEstetico { get; set; } = false;

        [Range(0, double.MaxValue)]
        public decimal CostoReacondicionamientoMecanico { get; set; } = 0.00m;

        [Range(0, double.MaxValue)]
        public decimal CostoReacondicionamientoEstetico { get; set; } = 0.00m;
    }

    public class ActualizarEtapaReacondicionamientoRequest
    {
        /// <summary>
        /// Etapa a actualizar: "mecanico", "estetico", "fotos", "listo"
        /// </summary>
        [Required]
        [RegularExpression("^(mecanico|estetico|fotos|listo)$",
            ErrorMessage = "Etapa debe ser: mecanico, estetico, fotos o listo")]
        public string Etapa { get; set; } = string.Empty;

        /// <summary>Indica si se marca inicio (true) o fin (false) de la etapa</summary>
        public bool Inicio { get; set; } = true;

    }

    public class LiberarVehiculoRequest
    {
        public DateTime? FechaLiberacion { get; set; }
    }

    // ============================================
    // RESPONSE DTOs — REACONDICIONAMIENTO ESTÉTICO
    // ============================================

    public class ReacondicionamientoEsteticoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? ReacondicionamientoId { get; set; }
    }

    public class ReacondicionamientoEsteticoDto
    {
        public int Id { get; set; }
        public int EncargadoEsteticaId { get; set; }
        public string EncargadoNombre { get; set; } = string.Empty;
        public int VehiculoId { get; set; }
        public string VehiculoInfo { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public int EstadoOrdenId { get; set; }
        public string EstadoOrden { get; set; } = string.Empty;
        public decimal CostoTotal { get; set; }
        public int TotalTrabajos { get; set; }
        public int TrabajosCompletados { get; set; }
        public decimal ProgresoGeneral { get; set; }
        public bool Activo { get; set; }
        public List<TrabajoEsteticoDto> Trabajos { get; set; } = new();
    }

    public class TrabajoEsteticoDto
    {
        public int Id { get; set; }
        public int ReacondicionamientoEsteticoId { get; set; }
        public string Trabajo { get; set; } = string.Empty;
        public string? EmpresaQueRealizara { get; set; }
        public DateTime? FechaHoraInicio { get; set; }
        public DateTime? FechaHoraTermino { get; set; }
        public string? IndicacionesTrabajo { get; set; }
        public int EstadoTrabajo { get; set; }
        public string EstadoTrabajoTexto => EstadoTrabajo switch
        {
            1 => "Pendiente",
            2 => "En Proceso",
            3 => "Completado",
            _ => "Desconocido"
        };
        public decimal CostoTrabajo { get; set; }
        public bool Activo { get; set; }
    }

    public class ListaReacondicionamientoEsteticoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ReacondicionamientoEsteticoDto> Reacondicionamientos { get; set; } = new();
    }

    public class ListaReacondicionmientosMecanicosEnProceso
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ReacondicionamientoMecanicoEnProceso> Reacondicionamientos { get; set; } = new();
    }

    public class ReacondicionamientoMecanicoEnProceso
    {
        public int Id { get; set; }
        public int VehiculoId { get; set; }
        public int AvaluoId { get; set; }
        public string VehiculoInfo { get; set; }= string.Empty;
        public string VIN { get; set; }=string.Empty;

        public DateTime? FechaInicioReacondicionamiento { get; set; }
        public int? OrdenReacondicionamientoId { get; set; }
    }



    // ============================================
    // RESPONSE DTOs — REACONDICIONAMIENTO GENERAL
    // ============================================

    public class ReacondicionamientoVehiculoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? ReacondicionamientoId { get; set; }
    }

    public class ReacondicionamientoVehiculoDto
    {
        public int Id { get; set; }
        public int AvaluoId { get; set; }
        public string VehiculoInfo { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;
        public DateTime FechaCompra { get; set; }

        // Mecánico
        public bool TieneReacondicionamientoMecanico { get; set; }
        public DateTime? FechaInicioReacondicionamientoMecanico { get; set; }
        public DateTime? FechaFinalizacionReacondicionamientoMecanico { get; set; }
        public decimal CostoReacondicionamientoMecanico { get; set; }
        //public int? ReacondicionamientoMecanicoId { get; set; }
        //public string? NumeroOrdenMecanica { get; set; }

        // Estético
        public bool TieneReacondicionamientoEstetico { get; set; }
        public DateTime? FechaInicioReacondicionamientoEstetico { get; set; }
        public DateTime? FechaFinalizacionReacondicionamientoEstetico { get; set; }
        public decimal CostoReacondicionamientoEstetico { get; set; }
        //public int? ReacondicionamientoEsteticoId { get; set; }
        //public decimal ProgresoEstetico { get; set; }

        // Fotografías
        public bool TieneFotografias { get; set; }
        public DateTime? FechaInicioTomaFotografias { get; set; }
        public DateTime? FechaFinalizacionTomaFotografias { get; set; }

        // Estado final
        public bool VehiculoListoVenta { get; set; }
        public DateTime? FechaVehiculoListo { get; set; }
        public DateTime? FechaLiberacionVehiculo { get; set; }

        public bool ReacondicionamientoMecanicoEnProceso { get; set; }
        public bool ReacondicionamientoEsteticoEnProceso { get; set; } 
        public bool VehiculoEnTomaFotografias { get; set; }

        // Total calculado
        public decimal CostoTotalReacondicionamiento { get; set; }
    }

    public class ListaReacondicionamientoVehiculoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ReacondicionamientoVehiculoDto> Reacondicionamientos { get; set; } = new();
    }
}