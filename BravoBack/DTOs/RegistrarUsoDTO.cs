using System.ComponentModel.DataAnnotations;

namespace BravoBack.DTOs
{
    public class RegistrarUsoDto
    {
        [Required]
        public int VehiculoId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Los kilómetros deben ser mayor a 0")]
        public int KilometrosRecorridos { get; set; }

        [Required]
        [Range(0.1, double.MaxValue, ErrorMessage = "Los litros deben ser mayor a 0")]
        public double LitrosConsumidos { get; set; }
    }
}