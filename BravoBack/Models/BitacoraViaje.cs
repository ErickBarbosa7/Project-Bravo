namespace BravoBack.Models;
public class BitacoraViaje
{
    public int Id { get; set; }
    public DateTime FechaSalida { get; set; }
    public DateTime? FechaLlegada { get; set; }
    public int KilometrajeSalida { get; set; }
    public int? KilometrajeLlegada { get; set; }
    public string? Destino { get; set; }
    public int VehiculoId { get; set; }
    public Vehiculo Vehiculo { get; set; } = null!;
    public string ConductorId { get; set; } = null!;
    public ApplicationUser Conductor { get; set; } = null!;
}