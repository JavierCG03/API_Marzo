using System.ComponentModel.DataAnnotations;

namespace CarSlineAPI.Models.DTOs
{
    // ============================================
    // VALIDACIÓN PREVIA
    // ============================================
    public class ValidarNumeroParteResponse
    {
        public bool Existe { get; set; }
        public string Message { get; set; } = string.Empty;
        public InventarioGeneralDto? Refaccion { get; set; } // Si existe, manda los datos
    }

    // ============================================
    // CREAR REFACCIÓN
    // ============================================
    public class CrearRefaccionInventarioRequest
    {
        // Datos de InventarioGeneral
        [Required(ErrorMessage = "El número de parte es requerido")]
        [MaxLength(50)]
        public string NumeroParte { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de refacción es requerido")]
        [MaxLength(50)]
        public string TipoRefaccion { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? Ubicacion { get; set; }

        public int CantidadMinima { get; set; } = 0;

        [Required]
        public string UnidadMedida { get; set; } = "Pieza";

        // Datos de EntradaInventario
        [Required(ErrorMessage = "La cantidad inicial es requerida")]
        [Range(1, int.MaxValue)]
        public int CantidadInicial { get; set; }

        [Required(ErrorMessage = "El almacenista es requerido")]
        public int AlmacenistaId { get; set; }

        // Compatibilidad (opcional)
        public CompatibilidadRequest? Compatibilidad { get; set; }
    }

    public class CompatibilidadRequest
    {
        [Required, MaxLength(50)]
        public string Marca { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Modelo { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Version { get; set; }

        [MaxLength(20)]
        public string? Motor { get; set; }

        [Required]
        public int AnioInicio { get; set; }

        [Required]
        public int AnioFin { get; set; }

        [MaxLength(200)]
        public string? Notas { get; set; }
    }

    public class CrearRefaccionInventarioResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? InventarioId { get; set; }
        public int? EntradaId { get; set; }
        public int? CompatibilidadId { get; set; }
    }

    // ============================================
    // ENTRADAS Y SALIDAS
    // ============================================
    public class RegistrarEntradaRequest
    {
        [Required]
        public int InventarioId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        [Required]
        public int AlmacenistaId { get; set; }
    }

    public class RegistrarSalidaRequest
    {
        [Required]
        public int InventarioId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        [Required]
        public int AlmacenistaId { get; set; }

        public int? TecnicoId { get; set; }

        public int? TrabajoId { get; set; }

        [Required]
        public string MotivoSalida { get; set; } = "Servicio";
    }

    public class MovimientoInventarioResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? MovimientoId { get; set; }
        public int StockActual { get; set; }
    }

    // ============================================
    // BUSCADOR POR VEHÍCULO
    // ============================================
    public class BuscarRefaccionVehiculoRequest
    {
        [Required, MaxLength(50)]
        public string TipoRefaccion { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Marca { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Modelo { get; set; } = string.Empty;

        // Opcionales — mejoran la búsqueda
        [MaxLength(50)]
        public string? Version { get; set; }

        public int? Anio { get; set; }

        [MaxLength(20)]
        public string? Motor { get; set; }
    }

    public class RefaccionDisponibleDto
    {
        public int InventarioId { get; set; }
        public string NumeroParte { get; set; } = string.Empty;
        public string TipoRefaccion { get; set; } = string.Empty;
        public string? Ubicacion { get; set; }
        public int StockDisponible { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public bool BajoStock { get; set; }
        public bool EsEquivalente { get; set; } // true si llegó por equivalencia
       public string Compatibilidad { get; set; } = string.Empty; // "Nissan Versa SL 2018-2023"
    }

    public class BuscarRefaccionVehiculoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<RefaccionDisponibleDto> Refacciones { get; set; } = new();
        public int TotalResultados { get; set; }
        public bool BusquedaAmpliada { get; set; } // true si se usó búsqueda sin año/motor
    }

    // ============================================
    // LISTADO GENERAL
    // ============================================
    public class InventarioGeneralDto
    {
        public int Id { get; set; }
        public string NumeroParte { get; set; } = string.Empty;
        public string TipoRefaccion { get; set; } = string.Empty;
        public string? Ubicacion { get; set; }
        public int Cantidad { get; set; }
        public int CantidadMinima { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public bool BajoStock => Cantidad <= CantidadMinima;
        public bool SinStock => Cantidad == 0;
    }

    public class InventarioPaginadoResponse
    {
        public bool Success { get; set; }
        public List<InventarioGeneralDto> Refacciones { get; set; } = new();
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalItems { get; set; }
        public int PorPagina { get; set; }
        public bool TienePaginaAnterior { get; set; }
        public bool TienePaginaSiguiente { get; set; }
        public int TotalBajoStock { get; set; }
        public int TotalSinStock { get; set; }
    }

    // ============================================
    // COMPATIBILIDADES Y EQUIVALENCIAS
    // ============================================
    public class AgregarCompatibilidadRequest
    {
        [Required]
        public int InventarioId { get; set; }

        [Required]
        public CompatibilidadRequest Compatibilidad { get; set; } = new();
    }

    public class AgregarEquivalenciaRequest
    {
        [Required]
        public int InventarioId { get; set; }

        [Required]
        public int InventarioEquivalenteId { get; set; }

    }

    public class CompatibilidadDto
    {
        public int Id { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string? Version { get; set; }
        public string? Motor { get; set; }
        public int AnioInicio { get; set; }
        public int AnioFin { get; set; }
        public string? Notas { get; set; }
        public string RangoAnios => $"{AnioInicio} - {AnioFin}";
    }

    public class EquivalenciaDto
    {
        public int Id { get; set; }
        public int InventarioEquivalenteId { get; set; }
        public string NumeroParteEquivalente { get; set; } = string.Empty;
        public string TipoRefaccion { get; set; } = string.Empty;
        public int StockEquivalente { get; set; }
    }

    public class DetalleRefaccionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public InventarioGeneralDto? Refaccion { get; set; }
        public List<CompatibilidadDto> Compatibilidades { get; set; } = new();
        public List<EquivalenciaDto> Equivalencias { get; set; } = new();
        public List<MovimientoDto> UltimosMovimientos { get; set; } = new();
    }

    public class MovimientoDto
    {
        public string Tipo { get; set; } = string.Empty; // "Entrada" o "Salida"
        public int Cantidad { get; set; }
        public DateTime Fecha { get; set; }
        public string Almacenista { get; set; } = string.Empty;
        public string? Motivo { get; set; }
    }

    // ============================================
    // ALERTAS
    // ============================================
    public class AlertasInventarioResponse
    {
        public bool Success { get; set; }
        public int TotalBajoStock { get; set; }
        public int TotalSinStock { get; set; }
        public List<InventarioGeneralDto> BajoStock { get; set; } = new();
        public List<InventarioGeneralDto> SinStock { get; set; } = new();
    }

    public class GenericInventarioResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}