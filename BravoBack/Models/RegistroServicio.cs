namespace BravoBack.Models; // <== Namespace actualizado
public class RegistroServicio
{
    public int Id { get; set; }
    public DateTime FechaServicio { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal? Costo { get; set; }
    public int KilometrajeEnServicio { get; set; }
    public int VehiculoId { get; set; }
    public Vehiculo Vehiculo { get; set; } = null!;
}