namespace BravoBack.DTOs
{
    public class ProyeccionGastosDto
    {
        public decimal CostoPromedioPorKm { get; set; }
        public int KmRecorridosUltimoMes { get; set; }
        public decimal PresupuestoSugerido { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}