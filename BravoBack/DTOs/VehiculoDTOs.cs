namespace BravoBack.DTOs;

public class CreateVehiculoDto
{
    public string Placa { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public int? Año { get; set; }
    public int KilometrajeActual { get; set; }
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
    public string Estado { get; set; } = string.Empty; 
    public int IntervaloServicioKm { get; set; }
    public int SiguienteServicioKm { get; set; }
}

// DTO para actualizar un vehículo
public class UpdateVehiculoDto : CreateVehiculoDto
{
    [System.Text.Json.Serialization.JsonIgnore]
    public int Id { get; set; }
}