using System.ComponentModel.DataAnnotations;

namespace BravoBack.DTOs;

// DTO para crear o actualizar un vehículo
public class CreateVehiculoDto
{
    [Required]
    public string Placa { get; set; } = string.Empty;
    
    [Required]
    public string Nombre { get; set; } = string.Empty;
    
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public int? Año { get; set; }
    
    [Required]
    public int KilometrajeActual { get; set; }
    
    [Required]
    public int IntervaloServicioKm { get; set; }
    
}

// DTO para mostrar un vehículo (la respuesta de la API)
public class VehiculoDto
{
    public int Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public int? Año { get; set; }
    public string? FotoUrl { get; set; }
    public int KilometrajeActual { get; set; }
    public string Estado { get; set; } = string.Empty; // "Disponible", etc.
    public int IntervaloServicioKm { get; set; }
    public int SiguienteServicioKm { get; set; }
}