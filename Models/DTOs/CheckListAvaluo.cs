using System.ComponentModel.DataAnnotations;

namespace CarSlineAPI.Models.DTOs
{
    public class GuardarCheckListAvaluoRequest
    {
        [Required]
        public int AvaluoId { get; set; }

        [Required]
        public int VigilanteId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Kilometraje { get; set; }

        [Range(0, 8)]
        public byte? Combustible { get; set; }

        // Accesorios
        public bool DuplicadoLlave { get; set; } = false;
        public bool BirloSeguridad { get; set; } = false;
        public bool CandadoSeguridad { get; set; } = false;
        public bool TapaCajuela { get; set; } = false;
        public bool CortinaCajuela { get; set; } = false;
        public bool Refaccion { get; set; } = false;
        public bool Gato { get; set; } = false;
        public bool Maneral { get; set; } = false;
        public bool LlaveLoCruz { get; set; } = false;
        public bool GanchoArrastre { get; set; } = false;
        public bool TaponGasolina { get; set; } = false;
        public bool Rines { get; set; } = false;
        public bool Tapones { get; set; } = false;
        public bool CentroRin { get; set; } = false;
        public bool RadioEstereo { get; set; } = false;
        public bool LimpiadorDelantero { get; set; } = false;
        public bool LimpiadorTrasero { get; set; } = false;
        public bool Viseras { get; set; } = false;
        public bool Cabeceras { get; set; } = false;
        public bool Tapetes { get; set; } = false;
        public bool FarosNiebla { get; set; } = false;
        public bool Bateria { get; set; } = false;
        public bool VarillasPickUp { get; set; } = false;
        public bool Encendedor { get; set; } = false;
        public bool AntenaRadio { get; set; } = false;
        public bool Maletin { get; set; } = false;
        public bool Cubresala { get; set; } = false;
        public bool Cubrevolante { get; set; } = false;

        [MaxLength(20)]
        public string? MarcaBateria { get; set; }

        [MaxLength(20)]
        public string? MarcaLlantaDelanteraDer { get; set; }
        [MaxLength(20)]
        public string? MarcaLlantaDelanteraIzq { get; set; }
        [MaxLength(20)]
        public string? MarcaLlantaTraseraDer { get; set; }
        [MaxLength(20)]
        public string? MarcaLlantaTraseraIzq { get; set; }

        [MaxLength(20)]
        public string? MedidaLlantaDelanteraDer { get; set; }
        [MaxLength(20)]
        public string? MedidaLlantaDelanteraIzq { get; set; }
        [MaxLength(20)]
        public string? MedidaLlantaTraseraDer { get; set; }
        [MaxLength(20)]
        public string? MedidaLlantaTraseraIzq { get; set; }

        [MaxLength(20)]
        public string? MarcaLlantaRefaccion { get; set; }
        [MaxLength(20)]
        public string? MedidaLlantaRefaccion { get; set; }

        public string? Comentarios { get; set; }
        public string? Observaciones { get; set; }
    }

    public class CheckListAvaluoDto
    {
        public int Id { get; set; }
        public int AvaluoId { get; set; }
        public int VigilanteId { get; set; }
        public string VigilanteNombre { get; set; } = string.Empty;
        public int Kilometraje { get; set; }
        public byte? Combustible { get; set; }

        public bool DuplicadoLlave { get; set; }
        public bool BirloSeguridad { get; set; }
        public bool CandadoSeguridad { get; set; }
        public bool TapaCajuela { get; set; }
        public bool CortinaCajuela { get; set; }
        public bool Refaccion { get; set; }
        public bool Gato { get; set; }
        public bool Maneral { get; set; }
        public bool LlaveLoCruz { get; set; }
        public bool GanchoArrastre { get; set; }
        public bool TaponGasolina { get; set; }
        public bool Rines { get; set; }
        public bool Tapones { get; set; }
        public bool CentroRin { get; set; }
        public bool RadioEstereo { get; set; }
        public bool LimpiadorDelantero { get; set; }
        public bool LimpiadorTrasero { get; set; }
        public bool Viseras { get; set; }
        public bool Cabeceras { get; set; }
        public bool Tapetes { get; set; }
        public bool FarosNiebla { get; set; }
        public bool Bateria { get; set; }
        public bool VarillasPickUp { get; set; }
        public bool Encendedor { get; set; }
        public bool AntenaRadio { get; set; }
        public bool Maletin { get; set; }
        public bool Cubresala { get; set; }
        public bool Cubrevolante { get; set; }

        public string? MarcaBateria { get; set; }
        public string? MarcaLlantaDelanteraDer { get; set; }
        public string? MarcaLlantaDelanteraIzq { get; set; }
        public string? MarcaLlantaTraseraDer { get; set; }
        public string? MarcaLlantaTraseraIzq { get; set; }
        public string? MedidaLlantaDelanteraDer { get; set; }
        public string? MedidaLlantaDelanteraIzq { get; set; }
        public string? MedidaLlantaTraseraDer { get; set; }
        public string? MedidaLlantaTraseraIzq { get; set; }
        public string? MarcaLlantaRefaccion { get; set; }
        public string? MedidaLlantaRefaccion { get; set; }

        public string? Comentarios { get; set; }
        public string? Observaciones { get; set; }
    }

    public class CheckListAvaluoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CheckListAvaluoDto? CheckList { get; set; }
    }

    public class CheckListAvaluoCompletoPdf
    {
        public AvaluoDto Avaluo { get; set; } 
        public CheckListAvaluoDto CheckList { get; set; }

    }
}