using BravoBack.Models;

namespace BravoBack.DTOs;

// DTO para crear un incidente
public class CreateIncidenteDto
{
    public int VehiculoId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public TipoIncidente TipoIncidente { get; set; }
    public IFormFile? Foto { get; set; } // El archivo de imagen es opcional
}

// DTO para mostrar un incidente
public class IncidenteDto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public string TipoIncidente { get; set; } = string.Empty;
    public string EstadoIncidente { get; set; } = string.Empty;
    public decimal? CostoReparacion { get; set; }
    public int VehiculoId { get; set; }
    public string ConductorNombre { get; set; } = string.Empty;
}