using System.ComponentModel.DataAnnotations;

namespace BravoBack.Models
{
    public class CatalogoVehiculo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Marca { get; set; }

        [Required]
        [MaxLength(50)]
        public string Modelo { get; set; }

        public int Anio { get; set; }

        [MaxLength(50)]
        public string Categoria { get; set; } // Ej: "Escolta", "Ejecutivo", "Carga"

        public int IntervaloServicioKm { get; set; } = 10000;

        [MaxLength(255)]
        public string FotoUrl { get; set; }
    }
}
