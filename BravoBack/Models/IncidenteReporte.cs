namespace BravoBack.Models; // <== Namespace actualizado
public class IncidenteReporte
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public TipoIncidente TipoIncidente { get; set; }
    public EstadoIncidente EstadoIncidente { get; set; }
    public decimal? CostoReparacion { get; set; }
    public int VehiculoId { get; set; }
    public Vehiculo Vehiculo { get; set; } = null!;
    public string ConductorId { get; set; } = null!;
    public ApplicationUser Conductor { get; set; } = null!;
}