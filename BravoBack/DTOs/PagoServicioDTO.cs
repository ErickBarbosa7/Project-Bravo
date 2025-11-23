using System.ComponentModel.DataAnnotations;

namespace BravoBack.DTOs
{
    public class PagoServicioDTO
    {
        [Required]
        public int VehiculoId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }
        public string Concepto { get; set; } = "Mantenimiento General";
    }
}