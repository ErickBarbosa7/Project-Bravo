using BravoBack.Models.Enums;

namespace BravoBack.DTOs
{
    public class ReporteMantenimientoDto
    {
        public EstatusMantenimiento Estatus { get; set; } // El Enum (0, 1, 2)
        public string Color { get; set; } = string.Empty; // Ayuda visual: "ROJO", "VERDE"
        public int KmRestantes { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}