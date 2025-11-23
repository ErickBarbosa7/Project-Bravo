using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BravoBack.Models.Enums; // Asegúrate de que el namespace de tu Enum sea correcto

namespace BravoBack.Models
{
    public class RegistroServicio
    {
        [Key]
        public int Id { get; set; }

        // Clave foránea
        public int VehiculoId { get; set; }

        [ForeignKey("VehiculoId")]
        public Vehiculo? Vehiculo { get; set; } 
        // -------------------------------------------

        public decimal MontoPagado { get; set; }

        public EstadoServicio Estado { get; set; } = EstadoServicio.Pendiente;

        public int KilometrajeServicio { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}