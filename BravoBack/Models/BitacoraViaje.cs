using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BravoBack.Models
{
    public class BitacoraViaje
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaSalida { get; set; }

        public DateTime? FechaLlegada { get; set; }

        public int KilometrajeSalida { get; set; }

        public int? KilometrajeLlegada { get; set; }

        public string? Destino { get; set; }

        // --- RELACIONES ---

        // Relación con Vehículo
        public int VehiculoId { get; set; }
        
        [ForeignKey("VehiculoId")]
        public Vehiculo Vehiculo { get; set; } = null!;

        // Relación con Conductor (Usuario)
        public string ConductorId { get; set; } = null!;
        
        [ForeignKey("ConductorId")]
        public ApplicationUser Conductor { get; set; } = null!;
    }
}