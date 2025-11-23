using BravoBack.Models.Enums;

namespace BravoBack.Models; 
public class Vehiculo
{
    public int Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public int? Anio { get; set; }
    public string? FotoUrl { get; set; }
    public int KilometrajeActual { get; set; }
    public EstadoVehiculo Estado { get; set; }
    public int IntervaloServicioKm { get; set; }
    public int SiguienteServicioKm { get; set; }
    public List<BitacoraViaje> BitacoraViajes { get; set; } = new();
    public List<RegistroServicio> RegistrosServicio { get; set; } = new();
    //public List<IncidenteReporte> Incidentes { get; set; } = new();
}