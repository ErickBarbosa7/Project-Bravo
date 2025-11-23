using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BravoBack.Models
{
    public class BitacoraUso
    {
        public int Id { get; set; }

        [Required]
        public int VehiculoId { get; set; }

        [Required]
        public string ConductorId { get; set; } = string.Empty;

        [Required]
        public int KilometrosRecorridos { get; set; }

        [Required]
        public double LitrosConsumidos { get; set; }

        public DateTime FechaUso { get; set; } = DateTime.UtcNow;

        // Relaciones
        [ForeignKey("VehiculoId")]
        public Vehiculo Vehiculo { get; set; }

        [ForeignKey("ConductorId")]
        public ApplicationUser Conductor { get; set; }
    }
}
