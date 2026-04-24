using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarSlineAPI.Models.DTOs
{

    public class CrearAvaluoRequest
    {
        [Required(ErrorMessage = "El ID del asesor es requerido")]
        public int AsesorId { get; set; }

        [Required(ErrorMessage = "El nombre completo es requerido")]
        [MaxLength(200)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de cliente es requerido")]
        [MaxLength(50)]
        public string TipoCliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido")]
        [MaxLength(20)]
        public string Telefono1 { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefono2 { get; set; }

        [Required(ErrorMessage = "La marca es requerida")]
        [MaxLength(50)]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "El modelo es requerido")]
        [MaxLength(50)]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La versión es requerida")]
        [MaxLength(100)]
        public string Version { get; set; } = string.Empty;

        [Required(ErrorMessage = "El año es requerido")]
        [Range(1900, 2100, ErrorMessage = "Año inválido")]
        public short Anio { get; set; }

        [MaxLength(30)]
        public string? Color { get; set; }

        [Required(ErrorMessage = "El VIN es requerido")]
        [MaxLength(17), MinLength(17, ErrorMessage = "El VIN debe tener 17 caracteres")]
        public string VIN { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? Placas { get; set; }

        [MaxLength(20)]
        public string? PlacasEdo { get; set; } 

        [Required(ErrorMessage = "El kilometraje es requerido")]
        [Range(0, int.MaxValue)]
        public int Kilometraje { get; set; }

        [MaxLength(200)]
        public string CuentaDeVehiculo { get; set; } = "No Aplica";

        [Range(0, double.MaxValue)]
        public decimal PrecioSolicitado { get; set; } = 0;
    }
    public class CrearAvaluoMecanicoRequest
    {

        [Required]
        public int AvaluoId { get; set; }

        [Required]
        public int TecnicoId { get; set; }

        [MaxLength(45)]
        public string Combustible { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Motor { get; set; } = string.Empty;
        public bool Turbo { get; set; } = false;

        public byte CantidadCilindros { get; set; } = 4;
        [MaxLength(45)]
        public string Transmision { get; set; } = string.Empty;


        [Required, MaxLength(20)]
        public string MarcaLlantasDelanteras { get; set; } = string.Empty;

        public byte? VidaUtilLlantasDelanteras { get; set; }

        [Required, MaxLength(20)]
        public string MarcaLlantasTraseras { get; set; } = string.Empty;

        public byte? VidaUtilLlantasTraseras { get; set; }

        [Column(TypeName = "TEXT")]
        public string? ComentariosAvaluoMecanico { get; set; }


        [Required, MinLength(1, ErrorMessage = "Debe agregar al menos una reparación")]
        public List<ReparacionItemRequest> Reparaciones { get; set; } = new();
    }
    public class CrearEquipamientoRequest
    {
        public int AvaluoId { get; set; }

        [Required]
        public int AsesorId { get; set; }

        // Equipamiento
        public bool Herramienta { get; set; } = false;
        public bool LLantaRefaccion { get; set; } = false;
        public bool BirloSeguridad { get; set; } = false;
        public bool Manuales { get; set; } = false;
        public bool DuplicadoLlave { get; set; } = false;
        public bool ACC { get; set; } = false;
        public bool Quemacocos { get; set; } = false;
        public bool EspejosElectricos { get; set; } = false;
        public bool SegurosElectricos { get; set; } = false;
        public bool CristalesElectricos { get; set; } = false;
        public bool AsientosElectricos { get; set; } = false;
        public bool FarosNiebla { get; set; } = false;
        public bool RinesAluminio { get; set; } = false;
        public bool ControlesVolante { get; set; } = false;
        public bool EstereoCD { get; set; } = false;
        public bool ABS { get; set; } = false;
        public bool DireccionAsistida { get; set; } = false;
        public bool BolsasAire { get; set; } = false;
        public bool Traccion4x4 { get; set; } = false;
        public bool Bluetooth { get; set; } = false;
        public bool USB { get; set; } = false;
        public bool Pantalla { get; set; } = false;
        public bool GPS { get; set; } = false;
        public byte CantidadPuertas { get; set; } = 2;
        public byte CantidadPasajeros { get; set; } = 1;

        [Required, MaxLength(150)]
        public string Vestiduras { get; set; } = string.Empty;

        [Column(TypeName = "TEXT")]
        public string? EquipoAdicional { get; set; }

        [Column(TypeName = "TEXT")]
        public string? ComentariosEquimapiento { get; set; }

    }
    public class CrearDocumentacionRequest
    {
        public int AvaluoId { get; set; }
        [Required]
        public int AsesorId { get; set; }

        // Documentos
        public bool CarnetServicios { get; set; } = false;
        public DateTime? UltimoServicioRegistrado { get; set; }
        public short? UltimaTenenciaPagada { get; set; }
        public short? UltimaVerificacionPagada { get; set; }
        public bool FacturaOriginal { get; set; } = false;
        public byte NumeroDuenos { get; set; } = 1;
        public byte Refacturaciones { get; set; } = 0;
        public bool DocumentacionCompleta { get; set; } = false;

        [Column(TypeName = "TEXT")]
        public string? ComentariosAvaluoDocumentos { get; set; }

    }
    public class CrearReparacionesRequest
    {
        [Required]
        public int AvaluoId { get; set; }

        [Required]
        public int TecnicoId{ get; set; }


        [Required, MinLength(1, ErrorMessage = "Debe agregar al menos una reparación")]
        public List<ReparacionItemRequest> Reparaciones { get; set; } = new();
    }
    public class ReparacionItemRequest
    {
        [Required(ErrorMessage = "La descripción es requerida")]
        [MaxLength(200)]
        public string Reparacion { get; set; } = string.Empty;

        public string? DescripcionReparacion { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CostoAproximado { get; set; }
    }
    public class AutorizarAvaluoRequest
    {
        [Required]
        [Range(0, double.MaxValue)]
        public decimal PrecioAutorizado { get; set; }

    }
    public class CancelarAvaluoRequest
    {
        public string MotivoCancelacion { get; set; } = string.Empty;
        public bool VehiculoApto { get; set; } = true;
    }
    public class DetallesComprasRequest
    {
        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        public int AsesorId { get; set; }

        /// <summary>
        /// Valores permitidos: "pendiente", "concretado", "cancelado"
        /// </summary>
        [Required]
        [RegularExpression("^(pendiente|concretado|investigado|cancelado)$",
            ErrorMessage = "TipoAvaluo debe ser: pendiente, concretado, cancelado o investigado")]
        public string TipoAvaluo { get; set; } = string.Empty;
    }

    // ============================================
    // RESPONSE DTOs
    // ============================================
    public class EstadisticasComprasResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AvaluoRealizados { get; set; }
        public int TomasConcretadas { get; set; }
        public int AvaluosCancelados { get; set; }
        public int  AvaluosPendientes { get; set; }
        public int AvaluosInvestigacion { get; set; }
        public List<EstadisticasCompradorDTO> Compradores { get; set; } = new();
    }

    public class DetallesComprasResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AsesorCompras { get; set; } = string.Empty;
        public string? TipodeAvaluos = string.Empty;
        public List<AvaluoDetalleAvaluoIdResponse?> Avaluos { get; set; } = new();
    }
    

    public class CrearAvaluoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? AvaluoId { get; set; }
    }
    public class CrearAvaluoMecanicoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? AvaluoMecanicoId { get; set; }
    }
    public class CrearEquipamientoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? EquipamientoId { get; set; }
    }
    public class CrearDocumentacionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? DocumentacionId { get; set; }
    }
    public class AgregarReparacionesResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    public class ReparacionesResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ReparacionDto> Reparaciones { get; set; } = new();
        public decimal TotalCostoReparaciones { get; set; }
    }
    public class FotosAvaluoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<FotoAvaluoDto> Fotos { get; set; } = new();
        public int CantidadFotos { get; set; }
    }
    public class MisAvaluosResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<AvaluoSimpleDto> Avaluos { get; set; } = new();
    }
    public class AvaluoSimpleDto
    {        
        public int Id { get; set; }
        public string Vendedor { get; set; } = string.Empty;
        public string VehiculoCompleto { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;
        public bool EquipamientoAvaluo { get; set; }
        public bool FotosAvaluo { get; set; }
        public bool AvaluoDocumentos { get; set; }
        public bool AvaluoMecanico { get; set; }
        public decimal PrecioSolicitado { get; set; }
        public decimal PrecioAutorizado { get; set; }
    }
    public class AvaluoDatosSimplesResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Vendedor { get; set; } = string.Empty;
        public string VehiculoCompleto { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;
    }
    public class AvaluoDetalleAvaluoIdResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AvaluoId { get; set; }
        public string Vendedor { get; set; } = string.Empty;
        public string VehiculoCompleto { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;
    }
    public class AvaluoCompletoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AvaluoId { get; set; }

        [Required]
        public AvaluoDto? Avaluo { get; set; }
        public EquipamientoDto? Equipamiento { get; set; }
        public AvaluoDocumentosDto? Documentos { get; set; }    
        public AvaluoMecanicoDto? AvaluoMecanico { get; set; }
        public List<ReparacionDto> Reparaciones { get; set; } = new(); 

    }

    // ============================================
    // DATA TRANSFER OBJECTS
    // ============================================
    public class EstadisticasCompradorDTO
    {
        public string AsesorCompras { get; set; } = string.Empty;
        public int AsesorId { get; set; }
        public int AvaluoRealizados { get; set; }
        public int TomasConcretadas { get; set; }
        public int AvaluosCancelados { get; set; }
        public int AvaluosPendientes { get; set; }
        public int AvaluosInvestigacion { get; set; }

    }
    public class AvaluoDto
    {
        public int Id { get; set; }
        public string AsesorNombre { get; set; } = string.Empty;
        public string TecnicoNombre { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string TipoCliente { get; set; } = string.Empty;
        public string Telefono1 { get; set; } = string.Empty;
        public string? Telefono2 { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public short Anio { get; set; }
        public string? Color { get; set; }
        public string VIN { get; set; } = string.Empty;
        public string? Placas { get; set; }
        public string? PlacasEdo { get; set; }
        public int Kilometraje { get; set; }
        public string CuentaDeVehiculo { get; set; } = string.Empty;
        public decimal PrecioSolicitado { get; set; }
        public decimal PrecioTratado { get; set; }
        public decimal CostoAproximadoReacondicionamiento { get; set; }
        public DateTime FechaAvaluo { get; set; }
        public bool VehiculoApto { get; set; }
        public bool Fotografias { get; set; }
        public decimal PrecioAutorizado { get; set; }
        public bool VehiculoTomadoRevision { get; set; }
        public bool VehiculoComprado { get; set; }
        public string? ComentariosCancelacion {  get; set; }
        public bool Activo { get; set; }

    }
    public class EquipamientoDto
    {
        public int Id { get; set; }
        public int AvaluoId { get; set; }
        public bool Herramienta { get; set; } 
        public bool LLantaRefaccion { get; set; } 
        public bool BirloSeguridad { get; set; } 
        public bool Manuales { get; set; } 
        public bool DuplicadoLlave { get; set; } 
        public bool ACC { get; set; }
        public bool Quemacocos { get; set; }
        public bool EspejosElectricos { get; set; }
        public bool SegurosElectricos { get; set; }
        public bool CristalesElectricos { get; set; }
        public bool AsientosElectricos { get; set; }
        public bool FarosNiebla { get; set; }
        public bool RinesAluminio { get; set; }
        public bool ControlesVolante { get; set; }
        public bool EstereoCD { get; set; }
        public bool ABS { get; set; }
        public bool DireccionAsistida { get; set; }
        public bool BolsasAire { get; set; }
        public bool Traccion4x4 { get; set; }
        public bool Bluetooth { get; set; }
        public bool USB { get; set; }
        public bool Pantalla { get; set; }
        public bool GPS { get; set; }
        public byte CantidadPuertas { get; set; }
        public byte CantidadPasajeros { get; set; }
        public string Vestiduras { get; set; } = string.Empty;
        public string? EquipoAdicional { get; set; }
        public string? ComentariosEquipamiento { get; set; }

    }
    public class AvaluoMecanicoDto
    {
        public int Id { get; set; }
        public string Combustible { get; set; } = string.Empty;
        public string Motor { get; set; } = string.Empty;
        public bool Turbo { get; set; } = false;
        public byte CantidadCilindros { get; set; }
        public string Transmision { get; set; } = string.Empty;
        public string MarcaLlantasDelanteras { get; set; } = string.Empty;
        public byte? VidaUtilLlantasDelanteras { get; set; }
        public string MarcaLlantasTraseras { get; set; } = string.Empty;
        public byte? VidaUtilLlantasTraseras { get; set; }
        public string? ComentariosAvaluoMecanico { get; set; }
    }
    public class AvaluoDocumentosDto
    {
        public int Id { get; set; }
        public bool CarnetServicios { get; set; }
        public DateTime? UltimoServicioRegistrado { get; set; }
        public short? UltimaTenenciaPagada { get; set; }
        public short? UltimaVerificacionPagada { get; set; }
        public bool FacturaOriginal { get; set; }
        public byte NumeroDuenos { get; set; }
        public byte Refacturaciones { get; set; } 
        public bool DocumentacionCompleta { get; set; }
        public string? ComentariosAvaluoDocumentos { get; set; }
    }
    public class ReparacionDto
    {
        public int Id { get; set; }
        public string ReparacionNecesaria { get; set; } = string.Empty;
        public string? DescripcionReparacion { get; set; } = string.Empty;
        public decimal CostoAproximado { get; set; }
    }
    public class FotoAvaluoDto
    {
        public int Id { get; set; }
        public int AvaluoId { get; set; }
        public string? TipoFoto { get; set; }
        public string? RutaFoto { get; set; }
        public DateTime Fecha { get; set; }
    }
    public class EvidenciaDto
    {
        public int Id { get; set; }
        public int OrdenGeneralId { get; set; }
        public string RutaImagen { get; set; }
        public string Descripcion { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public bool Activo { get; set; }
    }

}